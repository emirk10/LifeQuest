using System.Text.Json;
using LifeQuestAPI.Application.Abstractions.AI;
using LifeQuestAPI.Application.Abstractions.Gamification;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.Application.Features.UserTasks.Commands.CompleteUserTask;

public sealed class CompleteUserTaskCommandHandler
    : IRequestHandler<CompleteUserTaskCommandRequest, CompleteUserTaskCommandResponse>
{
    private readonly IUserTaskReadRepository _userTaskReadRepository;
    private readonly IUserTaskWriteRepository _userTaskWriteRepository;
    private readonly ITaskVerificationWriteRepository _verificationWriteRepository;
    private readonly IGamificationService _gamificationService;
    private readonly IImageVerificationService _imageVerificationService;
    private readonly IReflectionVerificationService _reflectionVerificationService;
    private readonly IAppUserReadRepository _userReadRepository;
    private readonly IUserTaskInteractionWriteRepository _interactionWriteRepository;
    private readonly IAchievementService _achievementService;

    public CompleteUserTaskCommandHandler(
        IUserTaskReadRepository userTaskReadRepository,
        IUserTaskWriteRepository userTaskWriteRepository,
        ITaskVerificationWriteRepository verificationWriteRepository,
        IGamificationService gamificationService,
        IImageVerificationService imageVerificationService,
        IReflectionVerificationService reflectionVerificationService,
        IAppUserReadRepository userReadRepository,
        IUserTaskInteractionWriteRepository interactionWriteRepository,
        IAchievementService achievementService)
    {
        _userTaskReadRepository = userTaskReadRepository;
        _userTaskWriteRepository = userTaskWriteRepository;
        _verificationWriteRepository = verificationWriteRepository;
        _gamificationService = gamificationService;
        _imageVerificationService = imageVerificationService;
        _reflectionVerificationService = reflectionVerificationService;
        _userReadRepository = userReadRepository;
        _interactionWriteRepository = interactionWriteRepository;
        _achievementService = achievementService;
    }

    public async Task<CompleteUserTaskCommandResponse> Handle(
        CompleteUserTaskCommandRequest request, CancellationToken cancellationToken)
    {
        // 1. Gorev kontrolu
        var userTask = await _userTaskReadRepository.Table
            .Include(ut => ut.AppTask)
                .ThenInclude(t => t.Category)
            .FirstOrDefaultAsync(ut => ut.Id == request.UserTaskId, cancellationToken);

        if (userTask == null)
            return new() { Success = false, Message = "Gorev bulunamadi." };

        if (userTask.AppUserId != request.UserId)
            return new() { Success = false, Message = "Bu gorev size ait degil." };

        if (userTask.CompletedAt.HasValue)
            return new() { Success = false, Message = "Bu gorev zaten tamamlanmis." };

        var task = userTask.AppTask;
        if (task == null)
            return new() { Success = false, Message = "Gorev detaylari bulunamadi." };

        // 2. Dogrulama katmani
        var verification = new Domain.Entities.TaskVerification
        {
            UserTaskId = userTask.Id,
            Type = task.VerificationType,
            SubmittedAt = DateTime.UtcNow
        };

        bool isVerified = true;
        string verificationMessage = "";

        switch (task.VerificationType)
        {
            case VerificationType.Photo:
                var photoResult = await HandlePhotoVerification(request, task, verification);
                isVerified = photoResult.isVerified;
                verificationMessage = photoResult.message;
                if (photoResult.earlyReturn != null) return photoResult.earlyReturn;
                break;

            case VerificationType.Reflection:
                var reflectionResult = await HandleReflectionVerification(request, task, verification);
                isVerified = reflectionResult.isVerified;
                verificationMessage = reflectionResult.message;
                if (reflectionResult.earlyReturn != null) return reflectionResult.earlyReturn;
                break;

            case VerificationType.Timer:
                var timerResult = HandleTimerVerification(request, task, verification);
                isVerified = timerResult.isVerified;
                verificationMessage = timerResult.message;
                if (timerResult.earlyReturn != null) return timerResult.earlyReturn;
                break;

            case VerificationType.PhotoAndReflection:
                // Once fotograf, sonra metin dogrula
                var photoRes = await HandlePhotoVerification(request, task, verification);
                if (photoRes.earlyReturn != null) return photoRes.earlyReturn;

                var refRes = await HandleReflectionVerification(request, task, verification);
                if (refRes.earlyReturn != null) return refRes.earlyReturn;

                isVerified = photoRes.isVerified && refRes.isVerified;
                verificationMessage = $"{photoRes.message} | {refRes.message}";
                break;

            case VerificationType.Quiz:
                var quizResult = HandleQuizVerification(request, verification);
                isVerified = quizResult.isVerified;
                verificationMessage = quizResult.message;
                if (quizResult.earlyReturn != null) return quizResult.earlyReturn;
                break;

            case VerificationType.None:
            default:
                isVerified = true;
                verificationMessage = "Dogrulama gerekmez.";
                break;
        }

        // 3. Dogrulama kaydini kaydet
        verification.Status = isVerified
            ? VerificationStatus.Approved
            : VerificationStatus.Rejected;

        await _verificationWriteRepository.AddAsync(verification);

        if (!isVerified)
        {
            await _verificationWriteRepository.SaveAsync();
            return new()
            {
                Success = false,
                Message = "Dogrulama basarisiz oldu.",
                VerificationMessage = verificationMessage
            };
        }

        // 4. Gorev tamamlama + XP kazanma
        int xpToEarn = task.BaseXP;
        userTask.CompletedAt = DateTime.UtcNow;
        userTask.Status = AppTaskStatus.Completed;
        userTask.EarnedXp = xpToEarn;

        await _userTaskWriteRepository.SaveAsync();
        await _gamificationService.AddExperienceAsync(request.UserId, xpToEarn, task.CategoryId);

        // 5. ML modeli için etkileşim verisini kaydet
        var user = await _userReadRepository.Table
            .Include(u => u.CategoryProgresses)
            .Include(u => u.UserTasks)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            
        if (user != null)
        {
            var userCatLevelObj = user.CategoryProgresses?.FirstOrDefault(c => c.CategoryId == task.CategoryId);
            int userCatLevel = userCatLevelObj?.Level ?? 1;
            
            var interaction = new UserTaskInteraction
            {
                AppUserId = request.UserId,
                AppTaskId = task.Id,
                UserLevel = (int)user.GeneralLevel,
                UserCategoryLevel = userCatLevel,
                TaskDifficulty = (int)task.Difficulty,
                TaskBaseXP = task.BaseXP,
                DayOfWeek = (int)DateTime.UtcNow.DayOfWeek,
                HourOfDay = DateTime.UtcNow.Hour,
                UserCompletedTaskCount = user.UserTasks?.Count(ut => ut.Status == AppTaskStatus.Completed) ?? 0,
                UserStreakDays = 0, // Şimdilik sabit
                WasCompleted = true,
                CompletionTimeHours = (float)(DateTime.UtcNow - userTask.AssignedAt).TotalHours,
                Rating = 5.0f // Varsayılan puan
            };

            await _interactionWriteRepository.AddAsync(interaction);
            await _interactionWriteRepository.SaveAsync();
        }

        // 6. Başarımları Kontrol Et
        await _achievementService.CheckAchievementsAsync(request.UserId);

        return new()
        {
            Success = true,
            Message = "Gorev basariyla dogrulandi ve tamamlandi!",
            EarnedXp = xpToEarn,
            VerificationMessage = verificationMessage
        };
    }

    // -------- DOGRULAMA YARDIMCI METODLARI --------

    private async Task<(bool isVerified, string message, CompleteUserTaskCommandResponse? earlyReturn)>
        HandlePhotoVerification(CompleteUserTaskCommandRequest request, AppTask task, Domain.Entities.TaskVerification verification)
    {
        if (string.IsNullOrEmpty(request.PhotoBase64))
        {
            return (false, "", new()
            {
                Success = false,
                Message = "Bu gorev icin fotograf kaniti gerekli.",
                VerificationMessage = task.VerificationHint ?? "Lutfen goreve ait bir fotograf yukleyin."
            });
        }

        var result = await _imageVerificationService.VerifyAsync(
            request.PhotoBase64,
            task.ExpectedImageCategory ?? task.Category?.Name ?? "general"
        );

        verification.AIConfidenceScore = result.Confidence;
        verification.AIAnalysisResult = JsonSerializer.Serialize(result.DetectedLabels);

        return (result.IsValid, result.Message, null);
    }

    private async Task<(bool isVerified, string message, CompleteUserTaskCommandResponse? earlyReturn)>
        HandleReflectionVerification(CompleteUserTaskCommandRequest request, AppTask task, Domain.Entities.TaskVerification verification)
    {
        if (string.IsNullOrEmpty(request.ReflectionText))
        {
            return (false, "", new()
            {
                Success = false,
                Message = "Bu gorev icin yansima yazisi gerekli.",
                VerificationMessage = $"En az {task.MinReflectionWords} kelime yazmaniz gerekiyor."
            });
        }

        var result = await _reflectionVerificationService.VerifyAsync(
            request.ReflectionText,
            task.MinReflectionWords > 0 ? task.MinReflectionWords : 10,
            task.Category?.Name
        );

        verification.ReflectionText = request.ReflectionText;
        verification.ReflectionWordCount = result.WordCount;
        verification.SentimentScore = result.QualityScore;
        verification.IsReflectionRelevant = result.RelevanceScore > 0.3f;

        return (result.IsValid, result.Message, null);
    }

    private static (bool isVerified, string message, CompleteUserTaskCommandResponse? earlyReturn)
        HandleTimerVerification(CompleteUserTaskCommandRequest request, AppTask task, Domain.Entities.TaskVerification verification)
    {
        int requiredSeconds = task.MinTimerMinutes * 60;

        if (!request.TimerSeconds.HasValue || request.TimerSeconds < requiredSeconds)
        {
            int remaining = requiredSeconds - (request.TimerSeconds ?? 0);
            int remainingMinutes = remaining / 60;

            return (false, "", new()
            {
                Success = false,
                Message = $"Zamanlayici suresi yetersiz. {remainingMinutes} dakika daha gerekli.",
                VerificationMessage = $"Minimum {task.MinTimerMinutes} dakika gerekli."
            });
        }

        verification.TimerStartedAt = DateTime.UtcNow.AddSeconds(-(request.TimerSeconds ?? 0));
        verification.TimerEndedAt = DateTime.UtcNow;
        verification.MinimumMinutesRequired = task.MinTimerMinutes;

        return (true, "Zamanlayici dogrulamasi basarili.", null);
    }

    private static (bool isVerified, string message, CompleteUserTaskCommandResponse? earlyReturn)
        HandleQuizVerification(CompleteUserTaskCommandRequest request, Domain.Entities.TaskVerification verification)
    {
        if (!request.QuizScore.HasValue || !request.QuizTotalQuestions.HasValue)
        {
            return (false, "", new()
            {
                Success = false,
                Message = "Bu gorev icin quiz tamamlanmali.",
                VerificationMessage = "Lutfen quiz sorularini cevaplayin."
            });
        }

        verification.QuizScore = request.QuizScore;
        verification.QuizTotalQuestions = request.QuizTotalQuestions;

        // En az %60 basari gerekli
        float successRate = (float)request.QuizScore.Value / request.QuizTotalQuestions.Value;
        bool isVerified = successRate >= 0.6f;

        string message = isVerified
            ? $"Quiz basarili! ({request.QuizScore}/{request.QuizTotalQuestions})"
            : $"Quiz basarisiz. En az %60 gerekli. ({request.QuizScore}/{request.QuizTotalQuestions})";

        return (isVerified, message, isVerified ? null : new()
        {
            Success = false,
            Message = message,
            VerificationMessage = "En az %60 basari orani gereklidir."
        });
    }
}
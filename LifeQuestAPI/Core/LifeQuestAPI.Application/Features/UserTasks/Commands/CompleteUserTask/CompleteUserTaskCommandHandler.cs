using LifeQuestAPI.Application.Abstractions.Gamification;
using LifeQuestAPI.Application.Abstractions.Social;
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
    private readonly IGamificationService _gamificationService;
    private readonly IAppUserReadRepository _userReadRepository;
    private readonly IUserTaskInteractionWriteRepository _interactionWriteRepository;
    private readonly IAchievementService _achievementService;
    private readonly IChallengeService _challengeService;

    public CompleteUserTaskCommandHandler(
        IUserTaskReadRepository userTaskReadRepository,
        IUserTaskWriteRepository userTaskWriteRepository,
        IGamificationService gamificationService,
        IAppUserReadRepository userReadRepository,
        IUserTaskInteractionWriteRepository interactionWriteRepository,
        IAchievementService achievementService,
        IChallengeService challengeService)
    {
        _userTaskReadRepository = userTaskReadRepository;
        _userTaskWriteRepository = userTaskWriteRepository;
        _gamificationService = gamificationService;
        _userReadRepository = userReadRepository;
        _interactionWriteRepository = interactionWriteRepository;
        _achievementService = achievementService;
        _challengeService = challengeService;
    }

    public async Task<CompleteUserTaskCommandResponse> Handle(
        CompleteUserTaskCommandRequest request, CancellationToken cancellationToken)
    {
        // 1. Görev kontrolü
        var userTask = await _userTaskReadRepository.Table
            .Include(ut => ut.AppTask)
                .ThenInclude(t => t.Category)
            .FirstOrDefaultAsync(ut => ut.Id == request.UserTaskId, cancellationToken);

        if (userTask == null)
            return new() { Success = false, Message = "Görev bulunamadı." };

        if (userTask.AppUserId != request.UserId)
            return new() { Success = false, Message = "Bu görev size ait değil." };

        if (userTask.CompletedAt.HasValue)
            return new() { Success = false, Message = "Bu görev zaten tamamlanmış." };

        var task = userTask.AppTask;
        if (task == null)
            return new() { Success = false, Message = "Görev detayları bulunamadı." };

        // 2. Görev tamamlama + XP kazanma
        int xpToEarn = task.BaseXP;
        userTask.CompletedAt = DateTime.UtcNow;
        userTask.Status = AppTaskStatus.Completed;
        userTask.EarnedXp = xpToEarn;

        await _userTaskWriteRepository.SaveAsync();
        await _gamificationService.AddExperienceAsync(request.UserId, xpToEarn, task.CategoryId);

        // 3. ML modeli için etkileşim verisini kaydet
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
                UserStreakDays = 0,
                WasCompleted = true,
                CompletionTimeHours = (float)(DateTime.UtcNow - userTask.AssignedAt).TotalHours,
                Rating = 5.0f
            };

            await _interactionWriteRepository.AddAsync(interaction);
            await _interactionWriteRepository.SaveAsync();
        }

        // 4. Başarımları ve Mücadeleleri kontrol et
        await _achievementService.CheckAchievementsAsync(request.UserId);
        await _challengeService.UpdateChallengeProgressAsync(request.UserId, task.CategoryId);

        return new()
        {
            Success = true,
            Message = "Görev başarıyla tamamlandı! 🎉",
            EarnedXp = xpToEarn
        };
    }
}
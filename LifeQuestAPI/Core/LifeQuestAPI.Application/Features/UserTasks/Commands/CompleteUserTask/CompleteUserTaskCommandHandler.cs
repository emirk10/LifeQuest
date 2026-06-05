using LifeQuestAPI.Application.Abstractions;
using LifeQuestAPI.Application.Abstractions.Gamification;
using LifeQuestAPI.Application.Abstractions.Notifications;
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
    private readonly IAppUserWriteRepository _userWriteRepository;
    private readonly IUserGoalReadRepository _goalReadRepository;
    private readonly IUserGoalWriteRepository _goalWriteRepository;
    private readonly IGoalMilestoneReadRepository _milestoneReadRepository;
    private readonly IGoalMilestoneWriteRepository _milestoneWriteRepository;
    private readonly IUserTaskInteractionWriteRepository _interactionWriteRepository;
    private readonly IAchievementService _achievementService;
    private readonly IChallengeService _challengeService;
    private readonly IAppNotificationService _notificationService;
    private readonly IUserBehaviorAnalysisService _behaviorAnalysisService;

    public CompleteUserTaskCommandHandler(
        IUserTaskReadRepository userTaskReadRepository,
        IUserTaskWriteRepository userTaskWriteRepository,
        IGamificationService gamificationService,
        IAppUserReadRepository userReadRepository,
        IAppUserWriteRepository userWriteRepository,
        IUserGoalReadRepository goalReadRepository,
        IUserGoalWriteRepository goalWriteRepository,
        IGoalMilestoneReadRepository milestoneReadRepository,
        IGoalMilestoneWriteRepository milestoneWriteRepository,
        IUserTaskInteractionWriteRepository interactionWriteRepository,
        IAchievementService achievementService,
        IChallengeService challengeService,
        IAppNotificationService notificationService,
        IUserBehaviorAnalysisService behaviorAnalysisService)
    {
        _userTaskReadRepository = userTaskReadRepository;
        _userTaskWriteRepository = userTaskWriteRepository;
        _gamificationService = gamificationService;
        _userReadRepository = userReadRepository;
        _userWriteRepository = userWriteRepository;
        _goalReadRepository = goalReadRepository;
        _goalWriteRepository = goalWriteRepository;
        _milestoneReadRepository = milestoneReadRepository;
        _milestoneWriteRepository = milestoneWriteRepository;
        _interactionWriteRepository = interactionWriteRepository;
        _achievementService = achievementService;
        _challengeService = challengeService;
        _notificationService = notificationService;
        _behaviorAnalysisService = behaviorAnalysisService;
    }

    public async Task<CompleteUserTaskCommandResponse> Handle(
        CompleteUserTaskCommandRequest request, CancellationToken cancellationToken)
    {
        // 1. Görev kontrolü
        var userTask = await _userTaskReadRepository.Table
            .Include(ut => ut.AppTask)
                .ThenInclude(t => t!.Category)
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
        
        // FEAT-002 & FEAT-008
        userTask.ReflectionText = request.ReflectionText;
        userTask.CustomTags = request.CustomTags;

        await _gamificationService.AddExperienceWithTaskAsync(request.UserId, xpToEarn, userTask, task.CategoryId);
        await _userTaskWriteRepository.SaveAsync();

        // 3. Kullanıcı sayaçları + Streak + XP Bonus hesapla (MISS-004 + IMP-001)
        var user = await _userReadRepository.Table
            .Include(u => u.CategoryProgresses)
            .Include(u => u.UserTasks)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user != null)
        {
            // SP-05: ActiveTaskCount ve WeeklyCompletedCount güncelle
            if (user.ActiveTaskCount > 0) user.ActiveTaskCount--;
            user.WeeklyCompletedCount++;

            // ── MISS-004: Streak hesaplama ──────────────────────────────
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var yesterday = today.AddDays(-1);

            if (user.LastTaskCompletedDate == null)
            {
                // İlk görev tamamlama
                user.CurrentStreak = 1;
            }
            else if (user.LastTaskCompletedDate == yesterday)
            {
                // Dün tamamlandı: seri devam ediyor
                user.CurrentStreak++;
            }
            else if (user.LastTaskCompletedDate < yesterday)
            {
                // 2+ gün önce: seri sıfırla
                user.CurrentStreak = 1;
            }
            // Bugün zaten tamamlandıysa streak değişmez

            user.LongestStreak = Math.Max(user.LongestStreak, user.CurrentStreak);
            user.LastTaskCompletedDate = today;

            // ── IMP-001: XP Bonus Sistemi ───────────────────────────────
            int bonusXp = 0;

            // Günün ilk görevi: +20 XP
            bool isFirstTaskToday = !user.UserTasks!
                .Any(ut => ut.Status == AppTaskStatus.Completed &&
                           ut.CompletedAt.HasValue &&
                           ut.CompletedAt.Value.Date == DateTime.UtcNow.Date &&
                           ut.Id != userTask.Id);
            if (isFirstTaskToday)
                bonusXp += 20;

            // FEAT-002: Reflection Bonusu (+5 XP)
            if (!string.IsNullOrWhiteSpace(request.ReflectionText))
                bonusXp += 5;

            // Geri dönüş bonusu: 3+ gün aradan sonra +50 XP
            bool isComebackBonus = user.LastTaskCompletedDate.HasValue &&
                                   user.LastTaskCompletedDate.Value < today.AddDays(-3);
            if (isComebackBonus)
                bonusXp += 50;

            // Streak çarpanı (temel XP üzerine)
            double streakMultiplier = 1.0;
            if (user.CurrentStreak > 30) streakMultiplier = 1.25;
            else if (user.CurrentStreak > 7) streakMultiplier = 1.10;

            int finalXp = (int)Math.Round(xpToEarn * streakMultiplier) + bonusXp;

            // XP farkını ekle (temel XP zaten eklendi, sadece farkı ekle)
            int extraXp = finalXp - xpToEarn;
            if (extraXp > 0)
                await _gamificationService.AddExperienceAsync(request.UserId, extraXp);

            xpToEarn = finalXp; // response için güncelle

            _userWriteRepository.Update(user);
            await _userWriteRepository.SaveAsync();

            // SP-05: Hedef otomatik ilerleme güncelleme
            await UpdateGoalProgressAsync(request.UserId, task.CategoryId, xpToEarn, cancellationToken);

            // ML modeli için etkileşim verisini kaydet
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
                UserStreakDays = user.CurrentStreak,
                WasCompleted = true,
                CompletionTimeHours = (float)(DateTime.UtcNow - userTask.AssignedAt).TotalHours,
                Rating = 5.0f
            };

            await _interactionWriteRepository.AddAsync(interaction);
            await _interactionWriteRepository.SaveAsync();
        }

        // 4. Başarımları ve Mücadeleleri kontrol et
        await _achievementService.CheckAchievementsAsync(request.UserId);
        await _challengeService.UpdateChallengeProgressAsync(request.UserId, task.CategoryId, xpToEarn);

        // 5. Kullanıcı davranış analizi tetikleme (gerçek zamanlı öğrenme)
        try
        {
            await _behaviorAnalysisService.AnalyzeAndUpdateUserBehaviorAsync(request.UserId, cancellationToken);
        }
        catch
        {
            // Analiz hatası görevin tamamlanmasını engellemesin
        }

        return new()
        {
            Success = true,
            Message = "Görev başarıyla tamamlandı! 🎉",
            EarnedXp = xpToEarn
        };
    }

    /// <summary>SP-05: Görev tamamlandığında ilgili aktif hedeflerin ilerlemesini günceller.</summary>
    private async Task UpdateGoalProgressAsync(Guid userId, Guid? categoryId, int earnedXp, CancellationToken ct)
    {
        var activeGoals = await _goalReadRepository.GetWhere(
            g => g.AppUserId == userId && !g.IsCompleted && !g.IsArchived, tracking: true)
            .ToListAsync(ct);

        foreach (var goal in activeGoals)
        {
            bool updated = false;

            // AutoTrackByCategory: Kategori eşleşiyorsa CurrentValue++
            if (goal.AutoTrackByCategory && categoryId.HasValue &&
                goal.LinkedCategoryIds != null && goal.LinkedCategoryIds.Contains(categoryId.Value))
            {
                goal.CurrentValue++;
                updated = true;
            }

            // AutoTrackByXP: CurrentValue += kazanılan XP
            if (goal.AutoTrackByXP)
            {
                goal.CurrentValue += earnedXp;
                updated = true;
            }

            if (!updated) continue;

            // Hedef tamamlandı mı?
            if (goal.CurrentValue >= goal.TargetValue && !goal.IsCompleted)
            {
                goal.IsCompleted = true;
                goal.CompletedAt = DateTime.UtcNow;

                await _notificationService.SendNotificationAsync(
                    userId,
                    "Hedef Tamamlandı! 🏆",
                    $"Tebrikler! '{goal.Title}' hedefini tamamladın!",
                    "GoalCompleted");
            }
            else
            {
                // Milestone kontrolü
                var milestones = await _milestoneReadRepository
                    .GetWhere(m => m.UserGoalId == goal.Id && !m.IsCompleted, tracking: true)
                    .OrderBy(m => m.OrderIndex)
                    .ToListAsync(ct);

                foreach (var milestone in milestones.Where(m => goal.CurrentValue >= m.TargetValue))
                {
                    milestone.IsCompleted = true;
                    milestone.CompletedAt = DateTime.UtcNow;

                    await _notificationService.SendNotificationAsync(
                        userId,
                        "Milestone Tamamlandı! 🎯",
                        $"'{goal.Title}' hedefinde '{milestone.Title}' adımını geçtin!",
                        "MilestoneCompleted");
                }
                await _milestoneWriteRepository.SaveAsync();
            }

            _goalWriteRepository.Update(goal);
        }

        await _goalWriteRepository.SaveAsync();
    }
}
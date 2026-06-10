using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LifeQuestAPI.Application.Abstractions.Gamification;
using LifeQuestAPI.Application.Abstractions.Notifications;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Domain.Entities.Identity;
using LifeQuestAPI.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.Infrastructure.Services.Gamification;

public sealed class GamificationService : IGamificationService
{
    private readonly IAppUserReadRepository _userReadRepository;
    private readonly IAppUserWriteRepository _userWriteRepository;
    private readonly IBadgeReadRepository _badgeReadRepository;
    private readonly IUserBadgeReadRepository _userBadgeReadRepository;
    private readonly IUserBadgeWriteRepository _userBadgeWriteRepository;
    private readonly IUserCategoryProgressReadRepository _categoryProgressReadRepository;
    private readonly IUserCategoryProgressWriteRepository _categoryProgressWriteRepository;
    private readonly IAppNotificationService _notificationService;

    public GamificationService(
        IAppUserReadRepository userReadRepository,
        IAppUserWriteRepository userWriteRepository,
        IBadgeReadRepository badgeReadRepository,
        IUserBadgeReadRepository userBadgeReadRepository,
        IUserBadgeWriteRepository userBadgeWriteRepository,
        IUserCategoryProgressReadRepository categoryProgressReadRepository,
        IUserCategoryProgressWriteRepository categoryProgressWriteRepository,
        IAppNotificationService notificationService)
    {
        _userReadRepository = userReadRepository;
        _userWriteRepository = userWriteRepository;
        _badgeReadRepository = badgeReadRepository;
        _userBadgeReadRepository = userBadgeReadRepository;
        _userBadgeWriteRepository = userBadgeWriteRepository;
        _categoryProgressReadRepository = categoryProgressReadRepository;
        _categoryProgressWriteRepository = categoryProgressWriteRepository;
        _notificationService = notificationService;
    }

    public async Task<AppUser> AddExperienceAsync(Guid userId, int xpAmount, Guid? categoryId = null)
    {
        return await AddExperienceWithTaskAsync(userId, xpAmount, null, categoryId);
    }

    
    
    
    
    public async Task<AppUser> AddExperienceWithTaskAsync(Guid userId, int baseXpAmount, UserTask? userTask, Guid? categoryId = null)
    {
        var user = await _userReadRepository.GetByIdAsync(userId.ToString(), tracking: true);
        if (user == null) throw new Exception("Kullanıcı bulunamadı!");

        
        decimal compensationRate = userTask?.XpCompensationRate ?? 1.0m;
        
        if (userTask?.IsCompensated == true)
            compensationRate = Math.Min(compensationRate + 0.10m, 1.0m);

        int finalXp = (int)(baseXpAmount * compensationRate);

        if (userTask != null)
            userTask.EarnedXp = finalXp;

        user.TotalXP += finalXp;
        user.GeneralLevel = CalculateLevel(user.TotalXP);
        user.LastActiveAt = DateTime.UtcNow;

        
        UpdateDailyTaskLimit(user);

        _userWriteRepository.Update(user);

        if (categoryId.HasValue)
            await HandleCategoryProgression(userId, categoryId.Value, finalXp);

        await _userWriteRepository.SaveAsync();
        await CheckAndAwardBadges(user.Id, (int)user.GeneralLevel);

        return user;
    }

    private async Task HandleCategoryProgression(Guid userId, Guid categoryId, int xpAmount)
    {
        var progress = await _categoryProgressReadRepository.Table
            .FirstOrDefaultAsync(cp => cp.AppUserId == userId && cp.CategoryId == categoryId);

        if (progress == null)
        {
            progress = new UserCategoryProgress
            {
                AppUserId = userId,
                CategoryId = categoryId,
                Level = 1,
                CurrentXp = 0,
                XpToNextLevel = 100
            };
            await _categoryProgressWriteRepository.AddAsync(progress);
        }

        progress.CurrentXp += xpAmount;

        while (progress.CurrentXp >= progress.XpToNextLevel)
        {
            progress.CurrentXp -= progress.XpToNextLevel;
            progress.Level++;
            progress.XpToNextLevel += 50;
        }

        await _categoryProgressWriteRepository.SaveAsync();
    }

    private async Task CheckAndAwardBadges(Guid userId, int currentLevel)
    {
        var user = await _userReadRepository.Table
            .Include(u => u.UserTasks)
            .Include(u => u.CategoryProgresses)
            .FirstOrDefaultAsync(u => u.Id == userId);
            
        if (user == null) return;

        var eligibleBadges = await _badgeReadRepository.GetAll(tracking: false).ToListAsync();
        
        var ownedBadgeIds = await _userBadgeReadRepository.Table
            .Where(ub => ub.AppUserId == userId)
            .Select(ub => ub.BadgeId)
            .ToListAsync();

        var completedTasksCount = user.UserTasks?.Count(ut => ut.Status == Domain.Enums.AppTaskStatus.Completed) ?? 0;

        foreach (var badge in eligibleBadges)
        {
            if (ownedBadgeIds.Contains(badge.Id)) continue;

            bool isEarned = badge.Type switch
            {
                BadgeType.Level => currentLevel >= badge.RequiredValue,
                BadgeType.Streak => user.CurrentStreak >= badge.RequiredValue,
                BadgeType.TaskCount => completedTasksCount >= badge.RequiredValue,
                BadgeType.CategoryMaster => badge.CategoryId.HasValue && 
                    (user.CategoryProgresses?.FirstOrDefault(cp => cp.CategoryId == badge.CategoryId.Value)?.Level ?? 0) >= badge.RequiredValue,
                BadgeType.FirstTime => completedTasksCount >= 1,
                _ => false
            };

            if (isEarned)
            {
                var newUserBadge = new UserBadge
                {
                    AppUserId = userId,
                    BadgeId = badge.Id,
                    EarnedAt = DateTime.UtcNow
                };
                await _userBadgeWriteRepository.AddAsync(newUserBadge);
                
                await _notificationService.SendNotificationAsync(
                    userId,
                    "Yeni Rozet Kazanıldı! 🏆",
                    $"Tebrikler! '{badge.Name}' rozetini kazandın!",
                    "BadgeEarned");
            }
        }

        await _userBadgeWriteRepository.SaveAsync();
    }

    private void UpdateDailyTaskLimit(AppUser user)
    {
        int level = (int)Math.Floor(user.GeneralLevel);
        
        int calculatedLimit = 3 + (level - 1) / 2;
        if (calculatedLimit > 10) calculatedLimit = 10;

        
        if (calculatedLimit > user.DailyTaskLimit)
        {
            user.DailyTaskLimit = calculatedLimit;
        }
    }

    private double CalculateLevel(int totalXp)
    {
        
        if (totalXp < 500) return 1.0 + (totalXp / 500.0);
        if (totalXp < 1500) return 2.0 + ((totalXp - 500) / 1000.0);
        if (totalXp < 4000) return 3.0 + ((totalXp - 1500) / 1250.0); 
        if (totalXp < 15000) return 5.0 + ((totalXp - 4000) / 2200.0); 
        
        
        return 10.0 + ((totalXp - 15000) / 5000.0);
    }
}

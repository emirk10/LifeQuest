using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LifeQuestAPI.Application.Abstractions.Gamification;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Domain.Entities.Identity;
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

    public GamificationService(
        IAppUserReadRepository userReadRepository,
        IAppUserWriteRepository userWriteRepository,
        IBadgeReadRepository badgeReadRepository,
        IUserBadgeReadRepository userBadgeReadRepository,
        IUserBadgeWriteRepository userBadgeWriteRepository,
        IUserCategoryProgressReadRepository categoryProgressReadRepository,
        IUserCategoryProgressWriteRepository categoryProgressWriteRepository)
    {
        _userReadRepository = userReadRepository;
        _userWriteRepository = userWriteRepository;
        _badgeReadRepository = badgeReadRepository;
        _userBadgeReadRepository = userBadgeReadRepository;
        _userBadgeWriteRepository = userBadgeWriteRepository;
        _categoryProgressReadRepository = categoryProgressReadRepository;
        _categoryProgressWriteRepository = categoryProgressWriteRepository;
    }

    public async Task<AppUser> AddExperienceAsync(Guid userId, int xpAmount, Guid? categoryId = null)
    {
        return await AddExperienceWithTaskAsync(userId, xpAmount, null, categoryId);
    }

    /// <summary>
    /// SP-01: XpCompensationRate ve IsCompensated bonus destekli XP ekleme.
    /// Ertelenen görevlerde rate=0.70, normal tamamlamada rate=1.0, telafi tamamlamada +%10 bonus.
    /// </summary>
    public async Task<AppUser> AddExperienceWithTaskAsync(Guid userId, int baseXpAmount, UserTask? userTask, Guid? categoryId = null)
    {
        var user = await _userReadRepository.GetByIdAsync(userId.ToString(), tracking: true);
        if (user == null) throw new Exception("Kullanıcı bulunamadı!");

        // XP hesabı: CompensationRate uygula
        decimal compensationRate = userTask?.XpCompensationRate ?? 1.0m;
        // Telafi bonusu: Postponed görev tamamlanıyorsa +%10
        if (userTask?.IsCompensated == true)
            compensationRate = Math.Min(compensationRate + 0.10m, 1.0m);

        int finalXp = (int)(baseXpAmount * compensationRate);

        if (userTask != null)
            userTask.EarnedXp = finalXp;

        user.TotalXP += finalXp;
        user.GeneralLevel = 1 + (user.TotalXP / 1000.0);
        user.LastActiveAt = DateTime.UtcNow;

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
        var eligibleBadges = await _badgeReadRepository
            .GetWhere(b => b.RequiredLevel <= currentLevel, tracking: false)
            .ToListAsync();

        var ownedBadges = await _userBadgeReadRepository
            .GetWhere(ub => ub.AppUserId == userId, tracking: false)
            .Select(ub => ub.BadgeId)
            .ToListAsync();

        foreach (var badge in eligibleBadges)
        {
            if (!ownedBadges.Contains(badge.Id))
            {
                var newUserBadge = new UserBadge
                {
                    AppUserId = userId,
                    BadgeId = badge.Id,
                    EarnedAt = DateTime.UtcNow
                };
                await _userBadgeWriteRepository.AddAsync(newUserBadge);
            }
        }

        await _userBadgeWriteRepository.SaveAsync();
    }
}
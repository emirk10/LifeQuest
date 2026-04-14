using System;
using System.Linq;
using System.Threading.Tasks;
using LifeQuestAPI.Application.Abstractions.Gamification;
using LifeQuestAPI.Application.Abstractions.Notifications;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LifeQuestAPI.Infrastructure.Services.Gamification;

public class AchievementService : IAchievementService
{
    private readonly IAppUserReadRepository _userReadRepository;
    private readonly IAchievementReadRepository _achievementReadRepository;
    private readonly IUserAchievementReadRepository _userAchievementReadRepository;
    private readonly IUserAchievementWriteRepository _userAchievementWriteRepository;
    private readonly IAppNotificationService _notificationService;
    private readonly ILogger<AchievementService> _logger;

    public AchievementService(
        IAppUserReadRepository userReadRepository,
        IAchievementReadRepository achievementReadRepository,
        IUserAchievementReadRepository userAchievementReadRepository,
        IUserAchievementWriteRepository userAchievementWriteRepository,
        IAppNotificationService notificationService,
        ILogger<AchievementService> logger)
    {
        _userReadRepository = userReadRepository;
        _achievementReadRepository = achievementReadRepository;
        _userAchievementReadRepository = userAchievementReadRepository;
        _userAchievementWriteRepository = userAchievementWriteRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task CheckAchievementsAsync(Guid userId)
    {
        var user = await _userReadRepository.Table
            .Include(u => u.UserTasks)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return;

        var allAchievements = await _achievementReadRepository.GetAll(false).ToListAsync();
        var userAchievementIds = await _userAchievementReadRepository.Table
            .Where(ua => ua.AppUserId == userId)
            .Select(ua => ua.AchievementId)
            .ToListAsync();

        var completedTasksCount = user.UserTasks?.Count(ut => ut.Status == Domain.Enums.AppTaskStatus.Completed) ?? 0;

        foreach (var ach in allAchievements)
        {
            // Eger onceden acildiysa atla
            if (userAchievementIds.Contains(ach.Id)) continue;

            bool isEarned = false;

            // Simple rule checks (Based on enum string or logic)
            // Varsayılarak AchievementType enum veya isimlendirme:
            if (ach.Title.Contains("Seviye") && user.GeneralLevel >= ach.RequiredCount)
            {
                isEarned = true;
            }
            else if (ach.Title.Contains("Görev") && completedTasksCount >= ach.RequiredCount)
            {
                isEarned = true;
            }
            // Dilersen burada AchievementType Enum'una göre net kontroller yapılabilir.

            if (isEarned)
            {
                var newUserAchievement = new UserAchievement
                {
                    AppUserId = userId,
                    AchievementId = ach.Id,
                    EarnedAt = DateTime.UtcNow,
                    IsCompleted = true
                };

                await _userAchievementWriteRepository.AddAsync(newUserAchievement);
                
                // Anlık Bildirim (SignalR + veritabanı)
                await _notificationService.SendNotificationAsync(
                    userId, 
                    $"🏆 Yeni Başarım: {ach.Title}!", 
                    $"Tebrikler! {ach.Description} görevini başarıyla tamamladın.", 
                    "Achievement");
                
                _logger.LogInformation($"Kullanıcı {userId} yeni başarım kazandı: {ach.Title}");
            }
        }
        
        await _userAchievementWriteRepository.SaveAsync();
    }
}

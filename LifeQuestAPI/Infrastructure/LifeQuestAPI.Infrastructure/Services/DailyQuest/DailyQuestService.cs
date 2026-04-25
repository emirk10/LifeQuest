using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LifeQuestAPI.Application.Abstractions;
using LifeQuestAPI.Application.Abstractions.DailyQuest;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Domain.Entities.Identity;
using LifeQuestAPI.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LifeQuestAPI.Infrastructure.Services.DailyQuest;
public class DailyQuestService : IDailyQuestService
{
    private readonly IAppUserReadRepository _userReadRepository;
    private readonly IAppTaskReadRepository _taskReadRepository;
    private readonly IUserTaskReadRepository _userTaskReadRepository;
    private readonly IUserTaskWriteRepository _userTaskWriteRepository;
    private readonly ILogger<DailyQuestService> _logger;
    private readonly ITaskRecommendationService _taskRecommendationService;
    private readonly IUserDailyMoodReadRepository _moodReadRepository;

    public DailyQuestService(
        ILogger<DailyQuestService> logger,
        IUserTaskWriteRepository userTaskWriteRepository,
        IUserTaskReadRepository userTaskReadRepository,
        IAppTaskReadRepository taskReadRepository,
        IAppUserReadRepository userReadRepository,
        IUserDailyMoodReadRepository moodReadRepository,
        ITaskRecommendationService taskRecommendationService)
    {
        _userReadRepository = userReadRepository;
        _taskReadRepository = taskReadRepository;
        _userTaskReadRepository = userTaskReadRepository;
        _userTaskWriteRepository = userTaskWriteRepository;
        _logger = logger;
        _moodReadRepository = moodReadRepository;
        _taskRecommendationService = taskRecommendationService;
    }

    public async Task DistributeDailyQuestsAsync()
    {
        _logger.LogInformation($"Toplu görev dağıtımı başlıyor... {DateTime.UtcNow}");

        var users = await _userReadRepository.GetAll(false)
            .Include(u => u.CategoryProgresses)
            .ToListAsync();

        foreach (var user in users)
        {
            // SP-04: Tatil modundaki kullanıcılara görev atama
            if (user.IsOnVacation)
            {
                _logger.LogDebug("DailyQuestWorker: {UserId} tatil modunda, görev atlanıyor.", user.Id);
                continue;
            }

            var hasTasksToday = await _userTaskReadRepository.Table
                .AnyAsync(ut => ut.AppUserId == user.Id && ut.AssignedAt.Date == DateTime.UtcNow.Date);

            if (!hasTasksToday)
            {
                await AssignTasksToSpecificUser(user);
            }
        }
    }

    public async Task<List<Guid>> AssignQuestsToUserAsync(Guid userId)
    {
        var user = await _userReadRepository.Table
            .Include(u => u.CategoryProgresses)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) throw new Exception("Kullanıcı bulunamadı.");

        var hasActiveTasks = await _userTaskReadRepository.Table
            .AnyAsync(ut => ut.AppUserId == userId
                && (ut.Status == AppTaskStatus.Suggested || ut.Status == AppTaskStatus.Selected || ut.Status == AppTaskStatus.InProgress));

        if (hasActiveTasks)
        {
            throw new Exception("Mevcut görevlerini tamamlamadan yeni görev alamazsın!");
        }

        return await AssignTasksToSpecificUser(user);
    }

    private async Task<List<Guid>> AssignTasksToSpecificUser(AppUser user)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todayMood = await _moodReadRepository
            .GetWhere(m => m.AppUserId == user.Id && m.Date == today, tracking: false)
            .FirstOrDefaultAsync();

        // SP-01: Akıllı öneri motorunu (ML.NET tabanlı) kullanarak en uygun 6 görevi seç
        // Bu servis kendi içinde UserTask kayıtlarını Suggested olarak oluşturur ve kaydeder.
        var recommendations = await _taskRecommendationService.GetContextualRecommendationsAsync(
            user.Id,
            todayMood?.MoodMode ?? MoodMode.Normal,
            DayTimeSlot.Morning, // Default değer, gerekirse saate göre hesaplanabilir
            todayMood?.EnergyLevel ?? 3,
            todayMood?.AvailableMinutes ?? 30,
            6);

        _logger.LogInformation($"✅ {user.UserName} kullanıcısına {recommendations.Count} akıllı görev önerildi.");

        return recommendations.Select(r => r.UserTaskId).ToList();
    }
}
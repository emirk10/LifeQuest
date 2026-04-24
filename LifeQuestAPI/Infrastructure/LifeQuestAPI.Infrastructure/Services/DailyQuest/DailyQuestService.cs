using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

    public DailyQuestService(
        ILogger<DailyQuestService> logger,
        IUserTaskWriteRepository userTaskWriteRepository,
        IUserTaskReadRepository userTaskReadRepository,
        IAppTaskReadRepository taskReadRepository,
        IAppUserReadRepository userReadRepository)
    {
        _userReadRepository = userReadRepository;
        _taskReadRepository = taskReadRepository;
        _userTaskReadRepository = userTaskReadRepository;
        _userTaskWriteRepository = userTaskWriteRepository;
        _logger = logger;
    }

    public async Task DistributeDailyQuestsAsync()
    {
        _logger.LogInformation($"Toplu görev dağıtımı başlıyor... {DateTime.UtcNow}");

        var users = await _userReadRepository.GetAll(false)
            .Include(u => u.CategoryProgresses)
            .ToListAsync();

        foreach (var user in users)
        {
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
        var completedTaskIds = await _userTaskReadRepository.Table
            .Where(ut => ut.AppUserId == user.Id && ut.Status == AppTaskStatus.Completed)
            .Select(ut => ut.AppTaskId)
            .ToListAsync();

        var allTasks = await _taskReadRepository.GetAll(false)
            .Where(t => t.IsActive)
            .ToListAsync();

        var suitableTasks = allTasks.Where(task =>
        {
            if (completedTaskIds.Contains(task.Id)) return false;

            if (task.MinLevel > user.GeneralLevel) return false;

            if (task.CategoryId != Guid.Empty)
            {
                var userCatProgress = user.CategoryProgresses
                    .FirstOrDefault(cp => cp.CategoryId == task.CategoryId);

                int userCatLevel = userCatProgress?.Level ?? 1;

                switch (task.Difficulty)
                {
                    case DifficultyLevel.Medium:
                        if (userCatLevel < 2) return false;
                        break;
                    case DifficultyLevel.Hard:
                        if (userCatLevel < 4) return false;
                        break;
                }
            }

            return true;
        }).ToList();

        // Eski ML tabanlı öneri kaldırıldı — SP-01 TaskRecommendationService kullanılacak
        // Basit random seçim (eski davranış korunuyor)
        var selectedTasks = suitableTasks.OrderBy(_ => Guid.NewGuid()).Take(6).ToList();

        if (!suitableTasks.Any()) return new List<Guid>();

        var newTasks = new List<UserTask>();

        foreach (var task in selectedTasks)
        {
            newTasks.Add(new UserTask
            {
                Id = Guid.NewGuid(),
                AppUserId = user.Id,
                AppTaskId = task.Id,
                Status = AppTaskStatus.Suggested,
                EarnedXp = 0,
                AssignedAt = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddHours(24)
            });
        }

        if (newTasks.Count > 0)
        {
            await _userTaskWriteRepository.AddRangeAsync(newTasks);
            await _userTaskWriteRepository.SaveAsync();
            _logger.LogInformation($"✅ {user.UserName} kullanıcısına {newTasks.Count} yeni görev atandı.");
        }

        return newTasks.Select(t => t.Id).ToList();
    }
}
using LifeQuestAPI.Application.Abstractions.Notifications;
using LifeQuestAPI.Application.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.API.BackgroundServices;

/// <summary>SP-05: Hedef hatırlatma bildirimleri. Her gün 09:00'da çalışır.</summary>
public class GoalReminderWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GoalReminderWorker> _logger;

    public GoalReminderWorker(IServiceProvider serviceProvider, ILogger<GoalReminderWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("GoalReminderWorker başlatıldı.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextRun = now.Date.AddDays(1).AddHours(6); // UTC 06:00 = TR 09:00
            var delay = nextRun - now;
            if (delay < TimeSpan.Zero) delay = TimeSpan.FromSeconds(10);

            await Task.Delay(delay, stoppingToken);

            try
            {
                using var scope = _serviceProvider.CreateScope();
                await SendGoalRemindersAsync(scope.ServiceProvider, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GoalReminderWorker hata aldı.");
            }
        }
    }

    private async Task SendGoalRemindersAsync(IServiceProvider services, CancellationToken ct)
    {
        var goalRepo = services.GetRequiredService<IUserGoalReadRepository>();
        var goalWriteRepo = services.GetRequiredService<IUserGoalWriteRepository>();
        var notificationService = services.GetRequiredService<IAppNotificationService>();

        var now = DateTime.UtcNow;
        var goals = await goalRepo.GetWhere(
            g => !g.IsCompleted && !g.IsArchived && g.ReminderEnabled, tracking: true)
            .ToListAsync(ct);

        var dayOfWeek = now.DayOfWeek;
        int remindedCount = 0;

        foreach (var goal in goals)
        {
            bool shouldRemind = goal.ReminderFrequency switch
            {
                "daily" => true,
                "weekly" => dayOfWeek == DayOfWeek.Monday,
                "on_lag" => IsGoalOnLag(goal, now),
                _ => false
            };

            if (!shouldRemind) continue;
            if (goal.LastReminderAt?.Date == now.Date) continue; // Bugün zaten gönderildi

            var progressPct = goal.TargetValue > 0
                ? (int)((double)goal.CurrentValue / goal.TargetValue * 100)
                : 0;

            await notificationService.SendNotificationAsync(
                goal.AppUserId,
                "Hedef Hatırlatması 🎯",
                $"'{goal.Title}' hedefinizde %{progressPct} tamamlandı. Devam et!",
                "GoalReminder");

            goal.LastReminderAt = now;
            goalWriteRepo.Update(goal);
            remindedCount++;
        }

        await goalWriteRepo.SaveAsync();
        _logger.LogInformation("GoalReminderWorker: {Count} hedef hatırlatması gönderildi.", remindedCount);
    }

    private static bool IsGoalOnLag(Domain.Entities.UserGoal goal, DateTime now)
    {
        var totalDays = (goal.EndDate - goal.StartDate).TotalDays;
        if (totalDays <= 0) return false;
        var elapsedDays = (now - goal.StartDate).TotalDays;
        var expectedProgress = elapsedDays / totalDays;
        var actualProgress = goal.TargetValue > 0 ? (double)goal.CurrentValue / goal.TargetValue : 0;
        return actualProgress < expectedProgress * 0.8;
    }
}

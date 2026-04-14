using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LifeQuestAPI.API.BackgroundServices;

public class WeeklyReportWorker : BackgroundService
{
    private readonly ILogger<WeeklyReportWorker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public WeeklyReportWorker(ILogger<WeeklyReportWorker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Haftalık Rapor İşçisi (WeeklyReportWorker) başlatıldı.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;

            // Pazar günü (Sunday) saat 23:00'te çalışsın
            if (now.DayOfWeek == DayOfWeek.Sunday && now.Hour == 23)
            {
                _logger.LogInformation("Haftalık raporlar hesaplanmaya başlanıyor...");
                await GenerateReportsAsync();
                
                // Bir kez çalıştıktan sonra sonraki haftaya kadar veya ertesi saate kadar uyusun
                await Task.Delay(TimeSpan.FromHours(2), stoppingToken);
            }
            else
            {
                // Her 30 dakikada bir kontrol et
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }
    }

    private async Task GenerateReportsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var userReadRepo = scope.ServiceProvider.GetRequiredService<IAppUserReadRepository>();
        var userTaskReadRepo = scope.ServiceProvider.GetRequiredService<IUserTaskReadRepository>();
        var reportWriteRepo = scope.ServiceProvider.GetRequiredService<IWeeklyReportWriteRepository>();
        var notificationService = scope.ServiceProvider.GetRequiredService<Application.Abstractions.Notifications.IAppNotificationService>();

        var weekEnd = DateTime.UtcNow;
        var weekStart = weekEnd.AddDays(-7);

        var users = await userReadRepo.GetAll(false).ToListAsync();

        foreach (var user in users)
        {
            var thisWeekTasks = await userTaskReadRepo.Table
                .Include(ut => ut.AppTask)
                .Where(ut => ut.AppUserId == user.Id && ut.AssignedAt >= weekStart)
                .ToListAsync();

            if (!thisWeekTasks.Any()) continue;

            var completedTasks = thisWeekTasks.Where(t => t.Status == Domain.Enums.AppTaskStatus.Completed).ToList();
            var failedTasks = thisWeekTasks.Where(t => t.Status == Domain.Enums.AppTaskStatus.Failed || t.DueDate < DateTime.UtcNow && t.Status != Domain.Enums.AppTaskStatus.Completed).ToList();

            var totalXpEarned = completedTasks.Sum(t => t.EarnedXp);

            // Basit AI/Hesaplama özetleri
            string insight = "Bu hafta harika bir iş çıkardın!";
            string recommendation = "Önümüzdeki hafta Sağlık kategorisine daha fazla odaklanabilirsin.";

            var categoryGroups = completedTasks
                .Where(t => t.AppTask != null)
                .GroupBy(t => t.AppTask.CategoryId)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .ToList();

            var report = new WeeklyReport
            {
                Id = Guid.NewGuid(),
                AppUserId = user.Id,
                WeekStartDate = weekStart,
                WeekEndDate = weekEnd,
                TasksCompleted = completedTasks.Count,
                TasksFailed = failedTasks.Count,
                TotalXPEarned = totalXpEarned,
                StreakDays = 0, // Günlük streak logu eklendiğinde buradan çekilebilir
                StrongestCategory = categoryGroups.FirstOrDefault()?.CategoryId.ToString() ?? "N/A",
                WeakestCategory = categoryGroups.LastOrDefault()?.CategoryId.ToString() ?? "N/A",
                AIInsight = insight,
                AIRecommendation = recommendation,
                OverallPerformanceScore = (thisWeekTasks.Count > 0) ? ((float)completedTasks.Count / thisWeekTasks.Count * 100) : 0,
                CategoryBreakdownJson = JsonSerializer.Serialize(categoryGroups),
                CreatedAt = DateTime.UtcNow
            };

            await reportWriteRepo.AddAsync(report);
            
            // Bildirim At
            await notificationService.SendNotificationAsync(
                user.Id, 
                "Haftalık Raporun Hazır! 📊", 
                $"Bu hafta {completedTasks.Count} görev tamamladın ve toplam {totalXpEarned} XP kazandın.", 
                "Report");
        }

        await reportWriteRepo.SaveAsync();
        _logger.LogInformation($"Haftalık raporlar {users.Count} kullanıcı için oluşturuldu ve kaydedildi.");
    }
}

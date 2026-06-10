using LifeQuestAPI.Application.Abstractions.Notifications;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LifeQuestAPI.API.BackgroundServices;






public sealed class GoalRevisionWorker : BackgroundService
{
    private readonly ILogger<GoalRevisionWorker> _logger;
    private readonly IServiceProvider _serviceProvider;

    private static readonly TimeZoneInfo TurkeyTz =
        TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time") ??
        TimeZoneInfo.CreateCustomTimeZone("Turkey Standard Time", TimeSpan.FromHours(3), "Turkey", "Turkey");

    public GoalRevisionWorker(ILogger<GoalRevisionWorker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("GoalRevisionWorker başlatıldı.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TurkeyTz);

            
            if (nowLocal.DayOfWeek == DayOfWeek.Sunday && nowLocal.Hour == 20)
            {
                _logger.LogInformation("GoalRevisionWorker: Hedef revizyonu başlıyor.");
                await RunRevisionAsync(stoppingToken);

                
                await Task.Delay(TimeSpan.FromHours(2), stoppingToken);
            }
            else
            {
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }
    }

    private async Task RunRevisionAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var userReadRepo = scope.ServiceProvider.GetRequiredService<IAppUserReadRepository>();
        var userTaskReadRepo = scope.ServiceProvider.GetRequiredService<IUserTaskReadRepository>();
        var notifService = scope.ServiceProvider.GetRequiredService<IAppNotificationService>();
        var chatWriteRepo = scope.ServiceProvider.GetRequiredService<IChatBotMessageWriteRepository>();

        var cutoff = DateTime.UtcNow.AddDays(-14);

        var users = await userReadRepo
            .GetWhere(u => u.UserGoals != null && u.UserGoals.Any() && !u.IsOnVacation, tracking: false)
            .ToListAsync(ct);

        foreach (var user in users)
        {
            try
            {
                var tasks = await userTaskReadRepo.Table
                    .Include(ut => ut.AppTask).ThenInclude(t => t!.Category)
                    .Where(ut => ut.AppUserId == user.Id && ut.AssignedAt >= cutoff)
                    .ToListAsync(ct);

                if (!tasks.Any()) continue;

                
                var categoryStats = tasks
                    .Where(t => t.AppTask?.Category != null)
                    .GroupBy(t => t.AppTask!.Category!.Name)
                    .Select(g => new
                    {
                        Category = g.Key,
                        Total = g.Count(),
                        Completed = g.Count(t => t.Status == AppTaskStatus.Completed),
                        Rate = g.Count() > 0
                            ? (double)g.Count(t => t.Status == AppTaskStatus.Completed) / g.Count()
                            : 0.0
                    })
                    .ToList();

                
                var weakGoals = categoryStats
                    .Where(s => s.Rate < 0.20 && s.Total >= 3 
                        && (user.UserGoals ?? new()).Any(g =>
                            g.Contains(s.Category, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                foreach (var weak in weakGoals)
                {
                    string message =
                        $"'{weak.Category}' hedefini son 2 haftadır zor buluyor gibisin " +
                        $"(tamamlama: %{(int)(weak.Rate * 100)}). Bu hedefi güncellemek ister misin?";

                    
                    var chatMsg = new Domain.Entities.ChatBotMessage
                    {
                        Id = Guid.NewGuid(),
                        AppUserId = user.Id,
                        Content = message,
                        SenderType = Domain.Enums.SenderType.Bot,
                        IsProactive = true,
                        Intent = "goal_revision",
                        
                        ActionsOfferedJson = new List<string>
                        {
                            $"{{\"type\":\"update_goal\",\"label\":\"Evet, güncelle\",\"payload\":\"{weak.Category}\"}}",
                            $"{{\"type\":\"keep_goal\",\"label\":\"Hayır, devam et\",\"payload\":\"{weak.Category}\"}}"
                        },
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await chatWriteRepo.AddAsync(chatMsg);
                    await chatWriteRepo.SaveAsync();

                    
                    await notifService.SendNotificationAsync(
                        user.Id,
                        "Hedef Revizyonu 🎯",
                        message,
                        "GoalRevision");

                    _logger.LogInformation(
                        "GoalRevisionWorker: {Category} → {UserId}", weak.Category, user.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GoalRevisionWorker: {UserId} için hata.", user.Id);
            }
        }
    }
}

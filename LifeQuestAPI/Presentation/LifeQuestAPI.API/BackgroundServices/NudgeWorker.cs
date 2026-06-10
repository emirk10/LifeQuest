using LifeQuestAPI.Application.Abstractions.Notifications;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LifeQuestAPI.API.BackgroundServices;





public sealed class NudgeWorker : BackgroundService
{
    private readonly ILogger<NudgeWorker> _logger;
    private readonly IServiceProvider _serviceProvider;

    
    private static readonly TimeZoneInfo TurkeyTz =
        TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time") ??
        TimeZoneInfo.CreateCustomTimeZone("Turkey Standard Time", TimeSpan.FromHours(3), "Turkey", "Turkey");

    
    private static readonly (TimeOnly Time, NudgeType Type)[] Schedule =
    [
        (new TimeOnly(13, 0), NudgeType.Midday),
        (new TimeOnly(19, 0), NudgeType.Evening),
        (new TimeOnly(21, 30), NudgeType.NightStreak)
    ];

    public NudgeWorker(ILogger<NudgeWorker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NudgeWorker başlatıldı.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var nowUtc = DateTime.UtcNow;
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, TurkeyTz);
            var nowTime = TimeOnly.FromDateTime(nowLocal);

            foreach (var (scheduleTime, nudgeType) in Schedule)
            {
                
                var diff = Math.Abs((nowTime - scheduleTime).TotalMinutes);
                if (diff <= 2)
                {
                    _logger.LogInformation("NudgeWorker: {NudgeType} zamanı geldi.", nudgeType);
                    await RunNudgeAsync(nudgeType, nowLocal, stoppingToken);
                    break;
                }
            }

            
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task RunNudgeAsync(NudgeType nudgeType, DateTime nowLocal, CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var userReadRepo = scope.ServiceProvider.GetRequiredService<IAppUserReadRepository>();
        var userTaskReadRepo = scope.ServiceProvider.GetRequiredService<IUserTaskReadRepository>();
        var chatWriteRepo = scope.ServiceProvider.GetRequiredService<IChatBotMessageWriteRepository>();
        var notifService = scope.ServiceProvider.GetRequiredService<IAppNotificationService>();

        var today = DateOnly.FromDateTime(nowLocal);

        
        var users = await userReadRepo
            .GetWhere(u => u.NudgeEnabled, tracking: false)
            .ToListAsync(ct);

        foreach (var user in users)
        {
            try
            {
                
                var chatReadRepo = scope.ServiceProvider.GetRequiredService<IChatBotMessageReadRepository>();
                var alreadySent = await chatReadRepo.GetWhere(
                    m => m.AppUserId == user.Id
                        && m.IsProactive
                        && m.Intent == nudgeType.ToString()
                        && DateOnly.FromDateTime(m.CreatedAt) == today,
                    tracking: false).AnyAsync(ct);

                if (alreadySent) continue;

                
                var todayTasks = await userTaskReadRepo.Table
                    .Where(ut => ut.AppUserId == user.Id)
                    .ToListAsync(ct);

                string? message = BuildNudgeMessage(nudgeType, user, todayTasks);
                if (message == null) continue;

                
                var chatMsg = new ChatBotMessage
                {
                    Id = Guid.NewGuid(),
                    AppUserId = user.Id,
                    Content = message,
                    SenderType = SenderType.Bot,
                    IsProactive = true,
                    Intent = nudgeType.ToString(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await chatWriteRepo.AddAsync(chatMsg);
                await chatWriteRepo.SaveAsync();

                
                await notifService.SendNotificationAsync(
                    user.Id,
                    "LifeQuest AI 🤖",
                    message,
                    "ProactiveNudge");

                _logger.LogInformation("NudgeWorker: {NudgeType} → {UserId}", nudgeType, user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "NudgeWorker: {UserId} için nudge gönderilemedi.", user.Id);
            }
        }
    }

    private static string? BuildNudgeMessage(
        NudgeType nudgeType,
        Domain.Entities.Identity.AppUser user,
        List<Domain.Entities.UserTask> todayTasks)
    {
        var name = user.Name;
        var completed = todayTasks.Count(t => t.Status == AppTaskStatus.Completed);
        var remaining = todayTasks.Count(t =>
            t.Status is AppTaskStatus.Suggested or AppTaskStatus.Selected or AppTaskStatus.InProgress);

        return nudgeType switch
        {
            NudgeType.Midday when completed == 0 =>
                $"Merhaba {name}! 🌞 Öğlen oldu ama henüz hiç göreve başlamadın. 15 dakikan varsa hızlı bir görev seni bekliyor!",

            NudgeType.Evening when completed == 0 =>
                $"Hey {name}! 🌆 Akşam oldu, bugün henüz hiç görev tamamlamadın. Streak'ini kurtarmak ister misin?",

            NudgeType.Evening when remaining > 0 =>
                $"{name}, bugün {completed} görev tamamladın! 💪 Hâlâ {remaining} görev bekliyor. Son bir hamle?",

            NudgeType.NightStreak when completed == 0 =>
                $"Gece olmadan önce {name}! 🌙 Sadece 5 dakikalık mini bir görev bile serini kurtarır. Hadi!",

            _ => null 
        };
    }
}

public enum NudgeType { Midday, Evening, NightStreak }

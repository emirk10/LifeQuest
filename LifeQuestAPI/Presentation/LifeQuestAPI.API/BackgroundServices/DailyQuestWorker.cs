using LifeQuestAPI.Application.Abstractions.DailyQuest;
using LifeQuestAPI.Application.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.API.BackgroundServices;

public class DailyQuestWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DailyQuestWorker> _logger;
    private readonly TimeSpan _period = TimeSpan.FromHours(24);

    public DailyQuestWorker(IServiceProvider serviceProvider, ILogger<DailyQuestWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker Başlatıldı.");

        using PeriodicTimer timer = new PeriodicTimer(_period);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dailyQuestService = scope.ServiceProvider.GetRequiredService<IDailyQuestService>();
                    await dailyQuestService.DistributeDailyQuestsAsync();

                    // SP-05: Günlük sayaçları sıfırla
                    await ResetDailyCountersAsync(scope.ServiceProvider, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Worker hata aldı!");
            }
        }
    }

    /// <summary>SP-05: Her gece TodayTaskCount ve OverLimitWarningShown sıfırla, Pazartesi WeeklyCompletedCount sıfırla.</summary>
    private async Task ResetDailyCountersAsync(IServiceProvider services, CancellationToken ct)
    {
        var userReadRepo = services.GetRequiredService<IAppUserReadRepository>();
        var userWriteRepo = services.GetRequiredService<IAppUserWriteRepository>();

        var users = await userReadRepo.GetAll(tracking: true).ToListAsync(ct);
        var isMonday = DateTime.UtcNow.DayOfWeek == DayOfWeek.Monday;

        foreach (var user in users)
        {
            user.TodayTaskCount = 0;
            user.OverLimitWarningShown = false;
            if (isMonday) user.WeeklyCompletedCount = 0;
            userWriteRepo.Update(user);
        }

        await userWriteRepo.SaveAsync();
        _logger.LogInformation("SP-05: {Count} kullanıcının günlük sayaçları sıfırlandı.", users.Count);
    }
}
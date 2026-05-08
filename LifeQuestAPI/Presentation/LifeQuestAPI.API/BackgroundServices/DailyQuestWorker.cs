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

        var configuration = _serviceProvider.GetRequiredService<IConfiguration>();
        var timeZoneId = configuration["AppTimezone"] ?? "Europe/Istanbul";
        TimeZoneInfo tz;
        try
        {
            tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            tz = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var nowUtc = DateTime.UtcNow;
            var nowInTz = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tz);
            var nextMidnightInTz = nowInTz.Date.AddDays(1);
            var nextMidnightUtc = TimeZoneInfo.ConvertTimeToUtc(nextMidnightInTz, tz);
            var delay = nextMidnightUtc - nowUtc;

            _logger.LogInformation("Bir sonraki görev dağıtımı {delay} sonra yapılacak. (TSİ: {time})", delay, nextMidnightInTz);

            try
            {
                await Task.Delay(delay, stoppingToken);

                if (stoppingToken.IsCancellationRequested) break;

                using (var scope = _serviceProvider.CreateScope())
                {
                    var dailyQuestService = scope.ServiceProvider.GetRequiredService<IDailyQuestService>();
                    await dailyQuestService.DistributeDailyQuestsAsync();

                    // SP-05: Günlük sayaçları sıfırla
                    await ResetDailyCountersAsync(scope.ServiceProvider, stoppingToken);
                }
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Worker hata aldı! 1 saat sonra tekrar denenecek.");
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }

    /// <summary>SP-05 + MISS-004: Gece yarısı sayaçları sıfırla, streak'leri güncelle. BUG-008: Paralel batch işleme.</summary>
    private async Task ResetDailyCountersAsync(IServiceProvider services, CancellationToken ct)
    {
        var userReadRepo = services.GetRequiredService<IAppUserReadRepository>();
        var userWriteRepo = services.GetRequiredService<IAppUserWriteRepository>();

        var users = await userReadRepo.GetAll(tracking: true).ToListAsync(ct);
        var isMonday = DateTime.UtcNow.DayOfWeek == DayOfWeek.Monday;
        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));

        // 500'lük batch'ler halinde standart (senkron) işleme
        var batches = users.Chunk(500);
        foreach (var batch in batches)
        {
            foreach (var user in batch)
            {
                user.TodayTaskCount = 0;
                user.OverLimitWarningShown = false;
                if (isMonday) user.WeeklyCompletedCount = 0;

                // MISS-004: Dün görev tamamlamayan kullanıcının streak'ini sıfırla
                // Tatil modundaki kullanıcıları atla (IsOnVacation kontrolü)
                var isOnVacation = user.GetType().GetProperty("IsOnVacation")?.GetValue(user) as bool?;
                if (isOnVacation != true)
                {
                    if (user.LastTaskCompletedDate.HasValue && user.LastTaskCompletedDate.Value < yesterday)
                    {
                        user.CurrentStreak = 0;
                    }
                }

                userWriteRepo.Update(user);
            }
        }

        await userWriteRepo.SaveAsync();
        _logger.LogInformation("SP-05 + MISS-004: {Count} kullanıcının günlük sayaçları ve streak'leri güncellendi.", users.Count);
    }
}
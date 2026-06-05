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
        
        var userIds = await userReadRepo.GetAll(tracking: false)
            .Select(u => u.Id)
            .ToListAsync(ct);

        var configuration = services.GetRequiredService<IConfiguration>();
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

        var localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        var isMonday = localTime.DayOfWeek == DayOfWeek.Monday;
        var yesterday = DateOnly.FromDateTime(localTime.AddDays(-1));

        // 100'lük batch'ler halinde paralel işleme
        var batches = userIds.Chunk(100).ToList();
        
        await Parallel.ForEachAsync(batches, new ParallelOptions { MaxDegreeOfParallelism = 4, CancellationToken = ct }, async (batch, token) =>
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var writeRepo = scope.ServiceProvider.GetRequiredService<IAppUserWriteRepository>();
                var readRepo = scope.ServiceProvider.GetRequiredService<IAppUserReadRepository>();
                
                var usersInBatch = await readRepo.GetWhere(u => batch.Contains(u.Id), tracking: true).ToListAsync(token);

                foreach (var user in usersInBatch)
                {
                    user.TodayTaskCount = 0;
                    user.OverLimitWarningShown = false;
                    if (isMonday) user.WeeklyCompletedCount = 0;

                    if (!user.IsOnVacation)
                    {
                        if (user.LastTaskCompletedDate.HasValue && user.LastTaskCompletedDate.Value < yesterday)
                        {
                            user.CurrentStreak = 0;
                        }
                    }
                    writeRepo.Update(user);
                }
                await writeRepo.SaveAsync();
            }
        });

        _logger.LogInformation("SP-05 + MISS-004: {Count} kullanıcının günlük sayaçları ve streak'leri paralel olarak güncellendi.", userIds.Count);
    }
}
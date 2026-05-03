using LifeQuestAPI.Application.Abstractions;
using LifeQuestAPI.Application.DTOs;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LifeQuestAPI.Infrastructure.Services.BehaviorAnalysis;

/// <summary>
/// SP-03: Son 30 günün UserTask davranışını analiz eder.
/// Haftalık rapor sonrası WeeklyReportWorker tarafından tetiklenir.
/// </summary>
public sealed class UserBehaviorAnalysisService : IUserBehaviorAnalysisService
{
    private readonly IAppUserReadRepository _userReadRepository;
    private readonly IAppUserWriteRepository _userWriteRepository;
    private readonly IUserTaskReadRepository _userTaskReadRepository;
    private readonly ILogger<UserBehaviorAnalysisService> _logger;

    public UserBehaviorAnalysisService(
        IAppUserReadRepository userReadRepository,
        IAppUserWriteRepository userWriteRepository,
        IUserTaskReadRepository userTaskReadRepository,
        ILogger<UserBehaviorAnalysisService> logger)
    {
        _userReadRepository = userReadRepository;
        _userWriteRepository = userWriteRepository;
        _userTaskReadRepository = userTaskReadRepository;
        _logger = logger;
    }

    public async Task AnalyzeAndUpdateUserBehaviorAsync(Guid userId, CancellationToken ct = default)
    {
        try
        {
            var cutoff = DateTime.UtcNow.AddDays(-30);

            var tasks = await _userTaskReadRepository.Table
                .Include(ut => ut.AppTask).ThenInclude(t => t!.Category)
                .Where(ut => ut.AppUserId == userId && ut.AssignedAt >= cutoff)
                .ToListAsync(ct);

            if (!tasks.Any()) return;

            var user = await _userReadRepository
                .GetWhere(u => u.Id == userId, tracking: true)
                .FirstOrDefaultAsync(ct);
            if (user == null) return;

            // ── AverageDailyActiveMinutes — EstimatedMinutes proxy olarak kullan ──
            var completedTasks = tasks.Where(t => t.Status == AppTaskStatus.Completed).ToList();
            user.AverageDailyActiveMinutes = completedTasks.Any()
                ? (int)completedTasks
                    .Where(t => t.AppTask?.EstimatedMinutes > 0)
                    .DefaultIfEmpty()
                    .Average(t => t?.AppTask?.EstimatedMinutes ?? 0)
                : 0;

            // ── MostSkippedCategory ────────────────────────────────────────
            var skippedByCategory = tasks
                .Where(t => t.Status is AppTaskStatus.Abandoned or AppTaskStatus.Expired)
                .GroupBy(t => t.AppTask?.Category?.Name ?? "Diğer")
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();
            user.MostSkippedCategory = skippedByCategory?.Key;

            // ── TotalPostponeCount ─────────────────────────────────────────
            user.TotalPostponeCount = tasks.Sum(t => t.PostponeCount);

            // ── PreferredMorningTime — en çok tamamlanan saat dilimi ───────
            var bestSlot = completedTasks
                .Where(t => t.DayTimeSlot.HasValue)
                .GroupBy(t => t.DayTimeSlot!.Value)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();
            user.PreferredMorningTime = SlotToTime(bestSlot);

            _userWriteRepository.Update(user);
            await _userWriteRepository.SaveAsync();

            _logger.LogInformation("SP-03: Davranış analizi güncellendi — UserId: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SP-03: Davranış analizi hatası — UserId: {UserId}", userId);
        }
    }

    public async Task<UserBehaviorSummaryDto> GetBehaviorSummaryAsync(
        Guid userId, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-30);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var tasks = await _userTaskReadRepository.Table
            .Include(ut => ut.AppTask).ThenInclude(t => t!.Category)
            .Where(ut => ut.AppUserId == userId && ut.AssignedAt >= cutoff)
            .ToListAsync(ct);

        var completed = tasks.Where(t => t.Status == AppTaskStatus.Completed).ToList();
        var abandoned = tasks.Where(t => t.Status == AppTaskStatus.Abandoned).ToList();

        // ── Tamamlama oranı ────────────────────────────────────────────────
        double completionRate = tasks.Count > 0
            ? (double)completed.Count / tasks.Count
            : 0.0;

        // ── En çok tamamlanan / atlanan kategori ──────────────────────────
        var mostCompleted = completed
            .GroupBy(t => t.AppTask?.Category?.Name ?? "Diğer")
            .OrderByDescending(g => g.Count())
            .FirstOrDefault()?.Key;

        var mostSkipped = tasks
            .Where(t => t.Status is AppTaskStatus.Abandoned or AppTaskStatus.Expired)
            .GroupBy(t => t.AppTask?.Category?.Name ?? "Diğer")
            .OrderByDescending(g => g.Count())
            .FirstOrDefault()?.Key;

        // ── En verimli saat dilimi ─────────────────────────────────────────
        var bestSlot = completed
            .Where(t => t.DayTimeSlot.HasValue)
            .GroupBy(t => t.DayTimeSlot!.Value)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();
        string bestTimeLabel = SlotToLabel(bestSlot);

        // ── Ortalama aktif dakika (EstimatedMinutes proxy) ─────────────────
        int avgMinutes = completed.Any(t => t.AppTask?.EstimatedMinutes > 0)
            ? (int)completed.Where(t => t.AppTask?.EstimatedMinutes > 0)
                .Average(t => t.AppTask!.EstimatedMinutes!.Value)
            : 0;

        // ── Streak hesaplama ───────────────────────────────────────────────
        var (current, longest) = CalculateStreaks(completed, today);

         // ── Son 7 gün günlük özet ──────────────────────────────────────────
        var weeklyProgress = Enumerable.Range(0, 7)
            .Select(i =>
            {
                var date = today.AddDays(-6 + i);
                var completedThatDay = tasks.Where(t =>
                    t.Status == AppTaskStatus.Completed && t.CompletedAt.HasValue &&
                    DateOnly.FromDateTime(t.CompletedAt.Value) == date).ToList();
                var dayTasks = tasks.Where(t =>
                    DateOnly.FromDateTime(t.AssignedAt) == date).ToList();

                return new DayProgressDto
                {
                    Date = date,
                    DayName = date.DayOfWeek switch
                    {
                        DayOfWeek.Monday    => "Pzt",
                        DayOfWeek.Tuesday   => "Sal",
                        DayOfWeek.Wednesday => "Çar",
                        DayOfWeek.Thursday  => "Per",
                        DayOfWeek.Friday    => "Cum",
                        DayOfWeek.Saturday  => "Cmt",
                        DayOfWeek.Sunday    => "Paz",
                        _                   => "?"
                    },
                    CompletedCount  = completedThatDay.Count,
                    PostponedCount  = dayTasks.Count(t => t.PostponeCount > 0),
                    AbandonedCount  = dayTasks.Count(t => t.Status == AppTaskStatus.Abandoned),
                    TotalXpEarned   = completedThatDay.Sum(t => t.EarnedXp)
                };
            })
            .ToList();

        // ── Insight mesajı ─────────────────────────────────────────────────
        string? insight = BuildInsightMessage(completionRate, mostSkipped, bestTimeLabel, current);

        return new UserBehaviorSummaryDto
        {
            AverageDailyActiveMinutes  = avgMinutes,
            BestTimeOfDay              = bestTimeLabel,
            MostCompletedCategory      = mostCompleted,
            MostSkippedCategory        = mostSkipped,
            CompletionRateLast30Days   = Math.Round(completionRate, 2),
            TotalCompletedLast30Days   = completed.Count,
            TotalPostponeCount         = tasks.Sum(t => t.PostponeCount),
            AbandonedTaskCount         = abandoned.Count,
            CurrentStreak              = current,
            LongestStreak              = longest,
            WeeklyProgress             = weeklyProgress,
            InsightMessage             = insight
        };
    }

    // ─── Yardımcı ─────────────────────────────────────────────────────────

    private static (int current, int longest) CalculateStreaks(
        List<Domain.Entities.UserTask> completed, DateOnly today)
    {
        var completedDays = completed
            .Where(t => t.CompletedAt.HasValue)
            .Select(t => DateOnly.FromDateTime(t.CompletedAt!.Value))
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        if (!completedDays.Any()) return (0, 0);

        int current = 0;
        var check = today;
        foreach (var day in completedDays)
        {
            if (day == check) { current++; check = check.AddDays(-1); }
            else if (day < check) break;
        }

        int longest = 0, streak = 1;
        var sorted = completedDays.OrderBy(d => d).ToList();
        for (int i = 1; i < sorted.Count; i++)
        {
            streak = sorted[i] == sorted[i - 1].AddDays(1) ? streak + 1 : 1;
            if (streak > longest) longest = streak;
        }

        return (current, Math.Max(longest, current));
    }

    private static TimeOnly? SlotToTime(DayTimeSlot? slot) => slot switch
    {
        DayTimeSlot.Morning   => new TimeOnly(7, 0),
        DayTimeSlot.Noon      => new TimeOnly(12, 0),
        DayTimeSlot.Afternoon => new TimeOnly(15, 0),
        DayTimeSlot.Evening   => new TimeOnly(19, 0),
        DayTimeSlot.Night     => new TimeOnly(22, 0),
        _                     => null
    };

    private static string SlotToLabel(DayTimeSlot? slot) => slot switch
    {
        DayTimeSlot.Morning   => "Sabah",
        DayTimeSlot.Noon      => "Öğle",
        DayTimeSlot.Afternoon => "Öğleden Sonra",
        DayTimeSlot.Evening   => "Akşam",
        DayTimeSlot.Night     => "Gece",
        _                     => "Bilinmiyor"
    };

    private static string? BuildInsightMessage(
        double completionRate, string? mostSkipped, string bestTime, int streak)
    {
        if (completionRate >= 0.8)
            return $"Harika! Son 30 günde %{(int)(completionRate * 100)} tamamlama oranıyla mükemmel gidiyorsun. 🏆";
        if (completionRate < 0.4 && mostSkipped != null)
            return $"'{mostSkipped}' kategorisinde zorlanıyorsun. Bu hafta oradan 1 küçük görev dene. 💪";
        if (streak >= 7)
            return $"{streak} günlük serine devam et! Yarın da bir görev tamamla. 🔥";
        if (bestTime != "Bilinmiyor")
            return $"En verimli zamanın {bestTime}. Görevlerini bu saate planlamayı dene. ⏰";
        return null;
    }
}

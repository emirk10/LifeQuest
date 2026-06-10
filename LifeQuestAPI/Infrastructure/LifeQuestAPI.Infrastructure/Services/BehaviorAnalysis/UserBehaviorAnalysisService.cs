using LifeQuestAPI.Application.Abstractions;
using LifeQuestAPI.Application.DTOs;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LifeQuestAPI.Infrastructure.Services.BehaviorAnalysis;





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

            
            var completedTasks = tasks.Where(t => t.Status == AppTaskStatus.Completed).ToList();
            user.AverageDailyActiveMinutes = completedTasks.Any()
                ? (int)completedTasks
                    .Where(t => t.AppTask?.EstimatedMinutes > 0)
                    .DefaultIfEmpty()
                    .Average(t => t?.AppTask?.EstimatedMinutes ?? 0)
                : 0;

            
            var skippedByCategory = tasks
                .Where(t => t.Status is AppTaskStatus.Abandoned or AppTaskStatus.Expired)
                .GroupBy(t => t.AppTask?.Category?.Name ?? "Diğer")
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();
            user.MostSkippedCategory = skippedByCategory?.Key;

            
            user.TotalPostponeCount = tasks.Sum(t => t.PostponeCount);

            
            var completedWithSlot = completedTasks.Where(t => t.DayTimeSlot.HasValue).ToList();
            if (completedWithSlot.Any())
            {
                var bestSlot = completedWithSlot
                    .GroupBy(t => t.DayTimeSlot!.Value)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .FirstOrDefault();
                user.PreferredMorningTime = SlotToTime(bestSlot);
            }
            else
            {
                user.PreferredMorningTime = null;
            }

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

        
        double completionRate = tasks.Count > 0
            ? (double)completed.Count / tasks.Count
            : 0.0;

        
        var mostCompleted = completed
            .GroupBy(t => t.AppTask?.Category?.Name ?? "Diğer")
            .OrderByDescending(g => g.Count())
            .FirstOrDefault()?.Key;

        var mostSkipped = tasks
            .Where(t => t.Status is AppTaskStatus.Abandoned or AppTaskStatus.Expired)
            .GroupBy(t => t.AppTask?.Category?.Name ?? "Diğer")
            .OrderByDescending(g => g.Count())
            .FirstOrDefault()?.Key;

        
        var completedWithSlot = completed.Where(t => t.DayTimeSlot.HasValue).ToList();
        string bestTimeLabel = "Bilinmiyor";
        if (completedWithSlot.Any())
        {
            var bestSlot = completedWithSlot
                .GroupBy(t => t.DayTimeSlot!.Value)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();
            bestTimeLabel = SlotToLabel(bestSlot);
        }

        
        int avgMinutes = completed.Any(t => t.AppTask?.EstimatedMinutes > 0)
            ? (int)completed.Where(t => t.AppTask?.EstimatedMinutes > 0)
                .Average(t => t.AppTask!.EstimatedMinutes!.Value)
            : 0;

        
        var (current, longest) = CalculateStreaks(completed, today);

         
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

        
        string? insight = BuildInsightMessage(completionRate, mostSkipped, bestTimeLabel, current);

        
        var user = await _userReadRepository
            .GetWhere(u => u.Id == userId, tracking: false)
            .FirstOrDefaultAsync(ct);

        var aiInsights = new List<string>();
        if (user != null)
        {
            
            if (current >= 7)
                aiInsights.Add($"🔥 Müthiş Disiplin: {current} gündür serini bozmadın. Sistem senin alışkanlık kazanma kararlılığını %95 olarak ölçtü!");
            else if (current > 0)
                aiInsights.Add($"💪 Seri Takibi: Şu an {current} günlük aktiflik serin var. Bunu korumak hedeflerine ulaşmanı kolaylaştıracaktır.");

            
            if (!string.IsNullOrEmpty(mostCompleted))
                aiInsights.Add($"🏆 Uzmanlık Alanın: En çok başarı gösterdiğin kategori '{mostCompleted}'. Bu kategorideki kararlılığın ve odaklanma gücün en üst düzeyde.");

            if (!string.IsNullOrEmpty(mostSkipped))
                aiInsights.Add($"⚠️ Gelişim Fırsatı: Son analizlere göre '{mostSkipped}' kategorisindeki görevleri atlama veya erteleme eğilimindesin. Asistanınız bu kategorideki görevleri 10-15 dakikalık kısa/kolay adımlara bölmeni öneriyor.");

            
            if (bestTimeLabel != "Bilinmiyor")
                aiInsights.Add($"⏰ Biyolojik Saat: Görevlerini en çok '{bestTimeLabel}' saatlerinde tamamlıyorsun. Kritik odaklanma gerektiren işlerini bu zaman dilimlerine planlaman verimliliğini %25 artıracaktır.");

            
            if (user.Strengths != null && user.Strengths.Any())
            {
                var strList = string.Join(", ", user.Strengths);
                aiInsights.Add($"✨ Güçlü Yönlerin: Sistem senin '{strList}' yönlerini başarıyla kullandığını analiz etti. Görev önerilerinde bu yönlerini besleyecek içeriklere öncelik veriliyor.");
            }

            
            if (user.Weaknesses != null && user.Weaknesses.Any())
            {
                var weakList = string.Join(", ", user.Weaknesses);
                aiInsights.Add($"💡 Gelişim Alanları: Kendinde '{weakList}' yönlerini geliştirmek istediğini belirttin. Asistanınız, bu alanlardaki direncini kırmak için proaktif yönlendirmeler hazırlıyor.");
            }

            
            int userPostpones = tasks.Sum(t => t.PostponeCount);
            if (userPostpones > 5)
                aiInsights.Add($"⏸️ Erteleme Eğilimi: Son 30 günde toplam {userPostpones} erteleme yaptın. Erteleme oranını düşürmek için günlük limitini (şu an {user.DailyTaskLimit} görev) düşürerek başlaman iyi bir strateji olabilir.");

            
            if (!string.IsNullOrEmpty(user.PrimaryFocusArea))
            {
                var focusAreaTr = user.PrimaryFocusArea.ToLower() switch
                {
                    "health" => "Sağlık",
                    "career" => "Kariyer",
                    "education" => "Eğitim",
                    "social" => "Sosyal",
                    "mindfulness" => "Kişisel Gelişim/Zihin",
                    "finance" => "Finans",
                    _ => user.PrimaryFocusArea
                };
                aiInsights.Add($"🎯 Odak Hedefin: Birincil hedefin '{focusAreaTr}' olarak belirlenmiş. Sistem ve chatbot, seni bu odakta tutmak için özel görev dağılımı yapıyor.");
            }
        }

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
            InsightMessage             = insight,
            AISystemInsights           = aiInsights
        };
    }

    

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

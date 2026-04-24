namespace LifeQuestAPI.Application.DTOs;

/// <summary>SP-03: Günlük ilerleme özeti (WeeklyProgress için)</summary>
public class DayProgressDto
{
    public DateOnly Date { get; set; }
    public string DayName { get; set; } = string.Empty; // Pazartesi, Salı...
    public int CompletedCount { get; set; }
    public int PostponedCount { get; set; }
    public int AbandonedCount { get; set; }
    public int TotalXpEarned { get; set; }
}

/// <summary>SP-03: Kullanıcı davranış analizi özeti (son 30 gün)</summary>
public class UserBehaviorSummaryDto
{
    // ─── Zaman & Aktivite ─────────────────────────────────────────────────
    public int AverageDailyActiveMinutes { get; set; }
    public string BestTimeOfDay { get; set; } = "Bilinmiyor"; // Sabah / Öğle / Akşam / Gece

    // ─── Kategori Analizi ─────────────────────────────────────────────────
    public string? MostCompletedCategory { get; set; }
    public string? MostSkippedCategory { get; set; }

    // ─── Tamamlama İstatistikleri ─────────────────────────────────────────
    public double CompletionRateLast30Days { get; set; } // 0.0 - 1.0
    public int TotalCompletedLast30Days { get; set; }
    public int TotalPostponeCount { get; set; }
    public int AbandonedTaskCount { get; set; }

    // ─── Streak ───────────────────────────────────────────────────────────
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }

    // ─── Haftalık Özet ────────────────────────────────────────────────────
    public List<DayProgressDto> WeeklyProgress { get; set; } = new();

    // ─── Chatbot Önerisi ──────────────────────────────────────────────────
    public string? InsightMessage { get; set; } // Sistem tarafından üretilen kişisel mesaj
}

using LifeQuestAPI.Domain.Entities.Common;
using LifeQuestAPI.Domain.Entities.Identity;
using LifeQuestAPI.Domain.Enums;

namespace LifeQuestAPI.Domain.Entities;

public sealed class UserTask : BaseEntity
{
    public Guid AppUserId { get; set; }
    public AppUser AppUser { get; set; } = default!;

    public Guid AppTaskId { get; set; }
    public AppTask AppTask { get; set; } = default!;

    public AppTaskStatus Status { get; set; } = AppTaskStatus.Suggested;

    public int EarnedXp { get; set; } = 0;

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public DateTime? DueDate { get; set; }

    // ─── SP-01: Yeni Alanlar ──────────────────────────────────────────────
    /// <summary>Görevin önerildiği zaman dilimi</summary>
    public DayTimeSlot? DayTimeSlot { get; set; }

    // ─── Erteleme Alanları ────────────────────────────────────────────────
    public bool IsPostponed { get; set; } = false;
    public int PostponeCount { get; set; } = 0;
    public DateOnly? PostponedTo { get; set; }
    public bool IsCompensated { get; set; } = false;
    public DateTime? CompensatedAt { get; set; }
    public string? SkipReason { get; set; }

    // ─── Enerji & Mood Snapshot ────────────────────────────────────────────
    /// <summary>Timer başladığında kullanıcının enerji seviyesi (1-5)</summary>
    public int? UserEnergyAtStart { get; set; }

    /// <summary>Timer başladığında kullanıcının modu</summary>
    public MoodMode? UserMoodAtStart { get; set; }

    // ─── Duraklatma ───────────────────────────────────────────────────────
    public DateTime? PausedAt { get; set; }
    public int TotalPausedMinutes { get; set; } = 0;

    // ─── XP Hesaplaması ───────────────────────────────────────────────────
    /// <summary>Normal tamamlama = 1.0, erteleme sonrası = 0.70, telafi = 1.10</summary>
    public decimal XpCompensationRate { get; set; } = 1.0m;
}
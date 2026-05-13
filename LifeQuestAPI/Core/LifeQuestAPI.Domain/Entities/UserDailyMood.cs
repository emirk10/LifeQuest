using LifeQuestAPI.Domain.Entities.Common;
using LifeQuestAPI.Domain.Entities.Identity;
using LifeQuestAPI.Domain.Enums;

namespace LifeQuestAPI.Domain.Entities;

public sealed class UserDailyMood : BaseEntity
{
    public Guid AppUserId { get; set; }
    public AppUser AppUser { get; set; } = default!;

    /// <summary>Kaydın ait olduğu gün</summary>
    public DateOnly Date { get; set; }

    /// <summary>Kullanıcının ruh hali — Energetic/Normal/Tired/Overwhelmed/LimitedTime</summary>
    public MoodMode MoodMode { get; set; } = MoodMode.Normal;

    /// <summary>Bugün kaç dakika müsait? (5-480)</summary>
    public int AvailableMinutes { get; set; } = 30;

    /// <summary>Enerji seviyesi 1-10</summary>
    public int EnergyLevel { get; set; } = 3;

    /// <summary>Phase 4: Günlük not / günlük kaydı</summary>
    public string? JournalEntry { get; set; }

    /// <summary>Phase 4: Uyku kalitesi (1-5)</summary>
    public int? SleepQuality { get; set; }

    /// <summary>Mood kaydedildiği zaman</summary>
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}

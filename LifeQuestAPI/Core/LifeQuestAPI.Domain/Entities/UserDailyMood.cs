using LifeQuestAPI.Domain.Entities.Common;
using LifeQuestAPI.Domain.Entities.Identity;
using LifeQuestAPI.Domain.Enums;

namespace LifeQuestAPI.Domain.Entities;

public sealed class UserDailyMood : BaseEntity
{
    public Guid AppUserId { get; set; }
    public AppUser AppUser { get; set; } = default!;

    
    public DateOnly Date { get; set; }

    
    public MoodMode MoodMode { get; set; } = MoodMode.Normal;

    
    public int AvailableMinutes { get; set; } = 30;

    
    public int EnergyLevel { get; set; } = 3;

    
    public string? JournalEntry { get; set; }

    
    public int? SleepQuality { get; set; }

    
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}

using LifeQuestAPI.Domain.Enums;

namespace LifeQuestAPI.Application.DTOs;

public class UserDailyMoodDto
{
    public Guid Id { get; set; }
    public Guid AppUserId { get; set; }
    public DateOnly Date { get; set; }
    public MoodMode MoodMode { get; set; }
    public int AvailableMinutes { get; set; }
    public int EnergyLevel { get; set; }
    public string? JournalEntry { get; set; }
    public int? SleepQuality { get; set; }
    public DateTime RecordedAt { get; set; }
}

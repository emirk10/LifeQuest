using LifeQuestAPI.Domain.Enums;

namespace LifeQuestAPI.Application.DTOs;

public class TaskRecommendationDto
{
    public Guid UserTaskId { get; set; }
    public Guid AppTaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int? EstimatedMinutes { get; set; }
    public int BaseXP { get; set; }
    public string Difficulty { get; set; } = string.Empty;
    public string? WhyThisTask { get; set; }
    public string? BadgeHint { get; set; }
    public string Status { get; set; } = "Suggested";

    /// <summary>0.0 - 1.0 arası uyum skoru</summary>
    public double MatchScore { get; set; }

    /// <summary>Neden önerildiğini Türkçe açıklayan metin</summary>
    public string RecommendationReason { get; set; } = string.Empty;
}

public class DailyTaskPoolDto
{
    public List<TaskRecommendationDto> Tasks { get; set; } = new();
    public MoodMode TodayMood { get; set; }
    public int AvailableMinutes { get; set; }
    public int EnergyLevel { get; set; }
    public DayTimeSlot TimeSlot { get; set; }
    public DateTime PoolGeneratedAt { get; set; }
}

public class PostponeResultDto
{
    public Guid UserTaskId { get; set; }
    public DateOnly PostponedTo { get; set; }
    public int PostponeCount { get; set; }
    public int RemainingPostpones { get; set; }
    public decimal XpCompensationRate { get; set; }
    public string Message { get; set; } = string.Empty;
}

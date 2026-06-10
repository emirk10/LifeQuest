using LifeQuestAPI.Domain.Entities.Common;
using LifeQuestAPI.Domain.Entities.Identity;

namespace LifeQuestAPI.Domain.Entities;

public sealed class WeeklyReport : BaseEntity
{
    public Guid AppUserId { get; set; }
    public AppUser AppUser { get; set; } = default!;

    public DateTime WeekStartDate { get; set; }
    public DateTime WeekEndDate { get; set; }

    public int TasksCompleted { get; set; }
    public int TasksFailed { get; set; }
    public int TotalXPEarned { get; set; }
    public int StreakDays { get; set; }

    public string StrongestCategory { get; set; } = string.Empty;
    public string WeakestCategory { get; set; } = string.Empty;

    
    public string? AIInsight { get; set; }
    public string? AIRecommendation { get; set; }
    public float? OverallPerformanceScore { get; set; }

    public string? CategoryBreakdownJson { get; set; }
}

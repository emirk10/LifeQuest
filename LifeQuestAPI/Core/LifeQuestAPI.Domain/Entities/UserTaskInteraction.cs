using LifeQuestAPI.Domain.Entities.Common;
using LifeQuestAPI.Domain.Entities.Identity;

namespace LifeQuestAPI.Domain.Entities;

public sealed class UserTaskInteraction : BaseEntity
{
    public Guid AppUserId { get; set; }
    public AppUser AppUser { get; set; } = default!;

    public Guid AppTaskId { get; set; }
    public AppTask AppTask { get; set; } = default!;

    
    public int UserLevel { get; set; }
    public int UserCategoryLevel { get; set; }
    public int TaskDifficulty { get; set; }
    public int TaskBaseXP { get; set; }
    public int DayOfWeek { get; set; }
    public int HourOfDay { get; set; }
    public int UserCompletedTaskCount { get; set; }
    public int UserStreakDays { get; set; }

    
    public bool WasCompleted { get; set; }
    public float CompletionTimeHours { get; set; }
    public float Rating { get; set; }
}

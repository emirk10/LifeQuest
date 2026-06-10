using LifeQuestAPI.Domain.Entities.Common;
using LifeQuestAPI.Domain.Entities.Identity;
using LifeQuestAPI.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace LifeQuestAPI.Domain.Entities;

public sealed class UserGoal : BaseEntity
{
    public Guid AppUserId { get; set; }
    public AppUser AppUser { get; set; } = default!;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public GoalType Type { get; set; }
    public int TargetValue { get; set; }
    public int CurrentValue { get; set; } = 0;

    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedAt { get; set; }
    public int BonusXP { get; set; } = 0;

    
    
    public string? Icon { get; set; }

    
    public string? Color { get; set; }

    
    public string? Why { get; set; }

    
    public string ProgressUnit { get; set; } = "görev";

    
    public int CompletionXP { get; set; } = 500;

    
    public Guid? CompletionBadgeId { get; set; }

    
    [Column(TypeName = "jsonb")]
    public List<Guid>? LinkedCategoryIds { get; set; }

    
    public bool AutoTrackByCategory { get; set; } = false;

    
    public bool AutoTrackByXP { get; set; } = false;

    
    public bool ReminderEnabled { get; set; } = true;

    
    public string ReminderFrequency { get; set; } = "weekly";

    public DateTime? LastReminderAt { get; set; }

    
    public bool IsArchived { get; set; } = false;

    public DateTime? ArchivedAt { get; set; }

    
    public ICollection<GoalMilestone> Milestones { get; set; } = default!;
}


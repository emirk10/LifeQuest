using System.ComponentModel.DataAnnotations.Schema;
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

    
    
    public DayTimeSlot? DayTimeSlot { get; set; }

    
    public bool IsPostponed { get; set; } = false;
    public int PostponeCount { get; set; } = 0;
    public DateOnly? PostponedTo { get; set; }
    public bool IsCompensated { get; set; } = false;
    public DateTime? CompensatedAt { get; set; }
    public string? SkipReason { get; set; }

    
    
    public int? UserEnergyAtStart { get; set; }

    
    public MoodMode? UserMoodAtStart { get; set; }

    
    public DateTime? PausedAt { get; set; }
    public int TotalPausedMinutes { get; set; } = 0;

    
    
    public decimal XpCompensationRate { get; set; } = 1.0m;

    
    public string? ReflectionText { get; set; }

    
    [Column(TypeName = "jsonb")]
    public List<string>? CustomTags { get; set; }

    
    public string? ShareToken { get; set; }
    public DateTime? ShareTokenExpiresAt { get; set; }
}
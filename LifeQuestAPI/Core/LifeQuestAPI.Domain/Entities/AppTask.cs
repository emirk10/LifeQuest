using System.ComponentModel.DataAnnotations.Schema;
using LifeQuestAPI.Domain.Entities.Common;
using LifeQuestAPI.Domain.Enums;

namespace LifeQuestAPI.Domain.Entities;

public sealed class AppTask : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = default!;

    public int MinLevel { get; set; }
    public int MaxLevel { get; set; }
    public int BaseXP { get; set; }

    public DifficultyLevel Difficulty { get; set; }
    public bool IsDaily { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsCustom { get; set; } = false;
    public Guid? CreatedByUserId { get; set; }

    
    
    public int? EstimatedMinutes { get; set; }

    
    [Column(TypeName = "jsonb")]
    public List<DayTimeSlot>? RecommendedTimeSlots { get; set; }

    
    public bool IsChunkable { get; set; } = false;

    
    public int? ChunkMinutes { get; set; }

    
    public int MinEnergyLevel { get; set; } = 1;

    
    public int MaxEnergyLevel { get; set; } = 5;

    
    public string? WhyThisTask { get; set; }

    
    public string? BadgeHint { get; set; }

    
    [Column(TypeName = "jsonb")]
    public List<string>? TargetedWeaknesses { get; set; }

    
    [Column(TypeName = "jsonb")]
    public List<string>? TargetedGoals { get; set; }

    
    [Column(TypeName = "jsonb")]
    public List<string>? Tags { get; set; }
}


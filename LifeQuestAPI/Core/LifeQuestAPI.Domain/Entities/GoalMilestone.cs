using LifeQuestAPI.Domain.Entities.Common;

namespace LifeQuestAPI.Domain.Entities;


public sealed class GoalMilestone : BaseEntity
{
    public Guid UserGoalId { get; set; }
    public UserGoal UserGoal { get; set; } = default!;

    public string Title { get; set; } = string.Empty;

    
    public int TargetValue { get; set; }

    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedAt { get; set; }

    public int BonusXP { get; set; } = 50;

    
    public int OrderIndex { get; set; }
}

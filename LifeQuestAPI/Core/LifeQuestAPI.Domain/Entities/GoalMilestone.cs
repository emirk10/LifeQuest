using LifeQuestAPI.Domain.Entities.Common;

namespace LifeQuestAPI.Domain.Entities;

/// <summary>SP-05: Büyük hedefleri alt adımlara böler. Her milestone tamamlanınca küçük kutlama.</summary>
public sealed class GoalMilestone : BaseEntity
{
    public Guid UserGoalId { get; set; }
    public UserGoal UserGoal { get; set; } = default!;

    public string Title { get; set; } = string.Empty;

    /// <summary>Bu milestone'un hedef değeri (genel hedefin alt kırılımı).</summary>
    public int TargetValue { get; set; }

    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedAt { get; set; }

    public int BonusXP { get; set; } = 50;

    /// <summary>Milestone sırası.</summary>
    public int OrderIndex { get; set; }
}

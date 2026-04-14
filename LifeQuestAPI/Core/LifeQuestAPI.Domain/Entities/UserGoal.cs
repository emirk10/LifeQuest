using LifeQuestAPI.Domain.Entities.Common;
using LifeQuestAPI.Domain.Entities.Identity;
using LifeQuestAPI.Domain.Enums;

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
    public int BonusXP { get; set; } = 0;
}

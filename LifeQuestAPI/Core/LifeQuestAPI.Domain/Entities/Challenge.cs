using LifeQuestAPI.Domain.Entities.Common;
using LifeQuestAPI.Domain.Entities.Identity;

namespace LifeQuestAPI.Domain.Entities;

public sealed class Challenge : BaseEntity
{
    public Guid ChallengerId { get; set; }
    public AppUser Challenger { get; set; } = default!;

    public Guid OpponentId { get; set; }
    public AppUser Opponent { get; set; } = default!;

    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }

    public string Title { get; set; } = string.Empty;
    public int TargetTaskCount { get; set; }

    public int ChallengerProgress { get; set; } = 0;
    public int OpponentProgress { get; set; } = 0;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid? WinnerId { get; set; }
    public int WinnerBonusXP { get; set; } = 200;
    public bool IsActive { get; set; } = true;
}

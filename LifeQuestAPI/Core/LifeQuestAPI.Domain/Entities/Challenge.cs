using LifeQuestAPI.Domain.Entities.Common;
using LifeQuestAPI.Domain.Entities.Identity;
using LifeQuestAPI.Domain.Enums;

namespace LifeQuestAPI.Domain.Entities;

public sealed class Challenge : BaseEntity
{
    public Guid ChallengerId { get; set; }
    public AppUser Challenger { get; set; } = default!;

    public Guid OpponentId { get; set; }
    public AppUser Opponent { get; set; } = default!;

    
    public ChallengeType ChallengeType { get; set; } = ChallengeType.TaskCount;
    public ChallengeStatus Status { get; set; } = ChallengeStatus.Invited;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    
    public Guid? TargetCategoryId { get; set; }
    public Category? TargetCategory { get; set; }

    
    public Guid? TargetAppTaskId { get; set; }
    public AppTask? TargetAppTask { get; set; }

    
    public int? TargetValue { get; set; }

    
    public int ChallengerProgress { get; set; } = 0;
    public int OpponentProgress { get; set; } = 0;

    
    public Guid? WinnerId { get; set; }
    public int WinnerXPBonus { get; set; } = 100;
    public int LoserXPPenalty { get; set; } = 0;

    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime InvitedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcceptedAt { get; set; }

    
    public string? ChallengerMessage { get; set; }
    public bool IsPublic { get; set; } = false;

    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool IsActive => Status == ChallengeStatus.Active;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int TargetTaskCount => TargetValue ?? 0;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int WinnerBonusXP => WinnerXPBonus;

    
    public ICollection<ChallengeParticipant> Participants { get; set; } = default!;
}


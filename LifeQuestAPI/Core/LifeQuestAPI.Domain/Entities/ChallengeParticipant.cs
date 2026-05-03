using LifeQuestAPI.Domain.Entities.Common;
using LifeQuestAPI.Domain.Entities.Identity;

namespace LifeQuestAPI.Domain.Entities;

/// <summary>SP-05: Mücadele katılımcısı — gelecekte grup mücadelesi için genişletilebilir.</summary>
public sealed class ChallengeParticipant : BaseEntity
{
    public Guid ChallengeId { get; set; }
    public Challenge Challenge { get; set; } = default!;

    public Guid UserId { get; set; }
    public AppUser User { get; set; } = default!;

    /// <summary>0 = Challenger, 1 = Opponent</summary>
    public int Role { get; set; }

    public int CurrentProgress { get; set; } = 0;
    public bool IsWinner { get; set; } = false;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

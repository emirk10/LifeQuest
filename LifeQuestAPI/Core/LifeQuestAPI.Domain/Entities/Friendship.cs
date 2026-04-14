using LifeQuestAPI.Domain.Entities.Common;
using LifeQuestAPI.Domain.Entities.Identity;
using LifeQuestAPI.Domain.Enums;

namespace LifeQuestAPI.Domain.Entities;

public sealed class Friendship : BaseEntity
{
    public Guid RequesterId { get; set; }
    public AppUser Requester { get; set; } = default!;

    public Guid AddresseeId { get; set; }
    public AppUser Addressee { get; set; } = default!;

    public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;
    public DateTime? AcceptedAt { get; set; }
}

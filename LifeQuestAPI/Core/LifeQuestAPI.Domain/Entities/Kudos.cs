using LifeQuestAPI.Domain.Entities.Common;
using LifeQuestAPI.Domain.Entities.Identity;

namespace LifeQuestAPI.Domain.Entities;

public sealed class Kudos : BaseEntity
{
    public Guid SenderId { get; set; }
    public AppUser Sender { get; set; } = default!;

    public Guid ReceiverId { get; set; }
    public AppUser Receiver { get; set; } = default!;

    public Guid UserTaskId { get; set; }
    public UserTask UserTask { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

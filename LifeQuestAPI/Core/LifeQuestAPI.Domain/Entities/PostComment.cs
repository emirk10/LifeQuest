using LifeQuestAPI.Domain.Entities.Common;
using LifeQuestAPI.Domain.Entities.Identity;

namespace LifeQuestAPI.Domain.Entities;

public sealed class PostComment : BaseEntity
{
    public Guid PostId { get; set; }
    public Post Post { get; set; } = default!;

    public Guid AppUserId { get; set; }
    public AppUser AppUser { get; set; } = default!;

    public string Content { get; set; } = default!;
}

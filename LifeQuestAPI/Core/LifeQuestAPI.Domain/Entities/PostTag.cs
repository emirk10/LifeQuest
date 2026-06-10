using LifeQuestAPI.Domain.Entities.Common;

namespace LifeQuestAPI.Domain.Entities;

public sealed class PostTag : BaseEntity
{
    public Guid PostId { get; set; }
    public Post Post { get; set; } = default!;

    public Guid AppTaskId { get; set; }
    public AppTask AppTask { get; set; } = default!;

    public Guid? UserTaskId { get; set; }
    public UserTask? UserTask { get; set; }

    
    public decimal CoordinateX { get; set; }

    
    public decimal CoordinateY { get; set; }
}

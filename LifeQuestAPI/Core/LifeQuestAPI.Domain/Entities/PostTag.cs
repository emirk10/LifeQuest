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

    /// <summary>Resim üzerindeki yatay konum, yüzde cinsinden (0–100).</summary>
    public decimal CoordinateX { get; set; }

    /// <summary>Resim üzerindeki dikey konum, yüzde cinsinden (0–100).</summary>
    public decimal CoordinateY { get; set; }
}

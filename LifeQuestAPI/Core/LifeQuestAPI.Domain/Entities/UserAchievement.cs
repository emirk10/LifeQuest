using LifeQuestAPI.Domain.Entities.Common;
using LifeQuestAPI.Domain.Entities.Identity;

namespace LifeQuestAPI.Domain.Entities;

public sealed class UserAchievement : BaseEntity
{
    public Guid AppUserId { get; set; }
    public AppUser AppUser { get; set; } = default!;

    public Guid AchievementId { get; set; }
    public Achievement Achievement { get; set; } = default!;

    public DateTime EarnedAt { get; set; } = DateTime.UtcNow;
    public int CurrentProgress { get; set; } = 0;
    public bool IsCompleted { get; set; } = false;
}

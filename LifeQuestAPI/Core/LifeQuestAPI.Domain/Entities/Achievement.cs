using LifeQuestAPI.Domain.Entities.Common;
using LifeQuestAPI.Domain.Enums;

namespace LifeQuestAPI.Domain.Entities;

public sealed class Achievement : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconPath { get; set; } = string.Empty;
    public AchievementType Type { get; set; }

    public int RequiredCount { get; set; }
    public int BonusXP { get; set; }
    public bool IsHidden { get; set; } = false;

    public ICollection<UserAchievement> UserAchievements { get; set; } = default!;
}

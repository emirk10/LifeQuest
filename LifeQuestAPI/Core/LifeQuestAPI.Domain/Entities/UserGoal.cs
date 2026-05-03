using LifeQuestAPI.Domain.Entities.Common;
using LifeQuestAPI.Domain.Entities.Identity;
using LifeQuestAPI.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace LifeQuestAPI.Domain.Entities;

public sealed class UserGoal : BaseEntity
{
    public Guid AppUserId { get; set; }
    public AppUser AppUser { get; set; } = default!;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public GoalType Type { get; set; }
    public int TargetValue { get; set; }
    public int CurrentValue { get; set; } = 0;

    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedAt { get; set; }
    public int BonusXP { get; set; } = 0;

    // ─── SP-05: Hedefler Sistemi Yeniden Tasarımı ─────────────────────────
    /// <summary>Emoji icon — kullanıcı seçer. Örn: 💪</summary>
    public string? Icon { get; set; }

    /// <summary>HEX renk kodu. Örn: #a78bfa</summary>
    public string? Color { get; set; }

    /// <summary>Bu hedef neden önemli? Kullanıcı yazar. Max 300 karakter.</summary>
    public string? Why { get; set; }

    /// <summary>İlerleme birimi: görev / km / sayfa / dakika / kez</summary>
    public string ProgressUnit { get; set; } = "görev";

    /// <summary>Hedef tamamlanınca kazanılacak bonus XP.</summary>
    public int CompletionXP { get; set; } = 500;

    /// <summary>Hedef tamamlanınca açılacak rozet (opsiyonel).</summary>
    public Guid? CompletionBadgeId { get; set; }

    /// <summary>Bu hedefle ilişkili kategori ID'leri — jsonb. Görev tamamlandığında bu hedef etkilenir.</summary>
    [Column(TypeName = "jsonb")]
    public List<Guid>? LinkedCategoryIds { get; set; }

    /// <summary>True ise bu kategorideki her görev tamamlandığında CurrentValue++ artar.</summary>
    public bool AutoTrackByCategory { get; set; } = false;

    /// <summary>True ise XP kazandığında CurrentValue += kazanılan XP.</summary>
    public bool AutoTrackByXP { get; set; } = false;

    /// <summary>Hatırlatma aktif mi?</summary>
    public bool ReminderEnabled { get; set; } = true;

    /// <summary>Hatırlatma frekansı: daily / weekly / on_lag</summary>
    public string ReminderFrequency { get; set; } = "weekly";

    public DateTime? LastReminderAt { get; set; }

    /// <summary>Silinmez, arşivlenir.</summary>
    public bool IsArchived { get; set; } = false;

    public DateTime? ArchivedAt { get; set; }

    // ─── Navigation Properties ─────────────────────────────────────────────
    public ICollection<GoalMilestone> Milestones { get; set; } = default!;
}


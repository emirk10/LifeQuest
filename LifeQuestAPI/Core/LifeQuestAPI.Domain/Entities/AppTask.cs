using System.ComponentModel.DataAnnotations.Schema;
using LifeQuestAPI.Domain.Entities.Common;
using LifeQuestAPI.Domain.Enums;

namespace LifeQuestAPI.Domain.Entities;

public sealed class AppTask : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = default!;

    public int MinLevel { get; set; }
    public int MaxLevel { get; set; }
    public int BaseXP { get; set; }

    public DifficultyLevel Difficulty { get; set; }
    public bool IsDaily { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsCustom { get; set; } = false;
    public Guid? CreatedByUserId { get; set; }

    // ─── SP-01: Yeni Alanlar ──────────────────────────────────────────────
    /// <summary>Görevin tahmini süresi (dakika)</summary>
    public int? EstimatedMinutes { get; set; }

    /// <summary>Hangi zaman dilimlerinde önerilsin — jsonb (ör: ["Morning","Noon"])</summary>
    [Column(TypeName = "jsonb")]
    public List<DayTimeSlot>? RecommendedTimeSlots { get; set; }

    /// <summary>Görev küçük parçalara bölünebilir mi?</summary>
    public bool IsChunkable { get; set; } = false;

    /// <summary>Parçalanabilir görevlerde minimum parça süresi (dakika)</summary>
    public int? ChunkMinutes { get; set; }

    /// <summary>Görevi yapabilmek için minimum enerji seviyesi (1-5)</summary>
    public int MinEnergyLevel { get; set; } = 1;

    /// <summary>Görevi yapabilmek için maksimum enerji seviyesi (1-5)</summary>
    public int MaxEnergyLevel { get; set; } = 5;

    /// <summary>Görevin neden önerildiğini açıklayan metin</summary>
    public string? WhyThisTask { get; set; }

    /// <summary>Rozet ipucu metni (ör: "3 gün üst üste yaparsan Demir İrade rozetini açarsın!")</summary>
    public string? BadgeHint { get; set; }

    /// <summary>Hangi zayıflıkları hedefliyor — jsonb</summary>
    [Column(TypeName = "jsonb")]
    public List<string>? TargetedWeaknesses { get; set; }

    /// <summary>Hangi hedefleri destekliyor — jsonb</summary>
    [Column(TypeName = "jsonb")]
    public List<string>? TargetedGoals { get; set; }

    /// <summary>Görev etiketleri — FEAT-008</summary>
    [Column(TypeName = "jsonb")]
    public List<string>? Tags { get; set; }
}

using LifeQuestAPI.Domain.Entities.Common;
using LifeQuestAPI.Domain.Entities.Identity;
using LifeQuestAPI.Domain.Enums;

namespace LifeQuestAPI.Domain.Entities;

public sealed class Challenge : BaseEntity
{
    public Guid ChallengerId { get; set; }
    public AppUser Challenger { get; set; } = default!;

    public Guid OpponentId { get; set; }
    public AppUser Opponent { get; set; } = default!;

    // ─── SP-05: Mücadele Tipi ve Durum ───────────────────────────────────
    public ChallengeType ChallengeType { get; set; } = ChallengeType.TaskCount;
    public ChallengeStatus Status { get; set; } = ChallengeStatus.Invited;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>CategoryFocus tipinde hedeflenen kategori.</summary>
    public Guid? TargetCategoryId { get; set; }
    public Category? TargetCategory { get; set; }

    /// <summary>SpecificTask tipinde hedeflenen görev.</summary>
    public Guid? TargetAppTaskId { get; set; }
    public AppTask? TargetAppTask { get; set; }

    /// <summary>TaskCount: kaç görev, XPRace: kaç XP. Null ise süre sonunda en yüksek kazanır.</summary>
    public int? TargetValue { get; set; }

    // ─── İlerleme ─────────────────────────────────────────────────────────
    public int ChallengerProgress { get; set; } = 0;
    public int OpponentProgress { get; set; } = 0;

    // ─── Sonuç ────────────────────────────────────────────────────────────
    public Guid? WinnerId { get; set; }
    public int WinnerXPBonus { get; set; } = 100;
    public int LoserXPPenalty { get; set; } = 0;

    // ─── Tarihler ─────────────────────────────────────────────────────────
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime InvitedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcceptedAt { get; set; }

    // ─── Davet ────────────────────────────────────────────────────────────
    public string? ChallengerMessage { get; set; }
    public bool IsPublic { get; set; } = false;

    // ─── Geriye Dönük Uyumluluk (Eski alanları eşliyoruz) ─────────────────
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool IsActive => Status == ChallengeStatus.Active;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int TargetTaskCount => TargetValue ?? 0;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int WinnerBonusXP => WinnerXPBonus;

    // ─── Navigation Properties ─────────────────────────────────────────────
    public ICollection<ChallengeParticipant> Participants { get; set; } = default!;
}


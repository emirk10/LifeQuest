using System.ComponentModel.DataAnnotations.Schema;
using LifeQuestAPI.Domain.Entities.Common;

namespace LifeQuestAPI.Domain.Entities.Identity;

public sealed class AppUser : BaseEntity
{
    // ─── Temel Kimlik Bilgileri ────────────────────────────────────────────
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string FullName => $"{Name} {Surname}";
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public byte[] PasswordHash { get; set; } = default!;
    public byte[] PasswordSalt { get; set; } = default!;
    public Guid AppRoleId { get; set; }
    public AppRole AppRole { get; set; } = default!;

    // ─── Gamification ─────────────────────────────────────────────────────
    public int TotalXP { get; set; } = 0;
    public double GeneralLevel { get; set; } = 1.0;

    // ─── Anket / Profil Alanları (SP-01) ──────────────────────────────────
    /// <summary>Kullanıcının hedefleri — jsonb (ör: ["fitness","career"])</summary>
    [Column("UserGoals", TypeName = "jsonb")]
    public List<string>? UserGoals { get; set; }

    /// <summary>Günlük ortalama müsait süre (dakika)</summary>
    public int? DailyAvailableMinutes { get; set; }

    /// <summary>Güçlü yönler — jsonb (ör: ["technology","writing"])</summary>
    [Column(TypeName = "jsonb")]
    public List<string>? Strengths { get; set; }

    /// <summary>Zayıf yönler — jsonb (ör: ["discipline","focus"])</summary>
    [Column(TypeName = "jsonb")]
    public List<string>? Weaknesses { get; set; }

    /// <summary>Motivasyon faktörü: leveling/badges/social/streaks/reports</summary>
    public string? MotivationFactor { get; set; }

    /// <summary>Haftada kaç görev tercih ediyor</summary>
    public int? PreferredTasksPerWeek { get; set; }

    /// <summary>Zorluk tercihi: easy/medium/hard/mixed</summary>
    public string? DifficultyPreference { get; set; }

    /// <summary>Profil anketi tamamlandı mı?</summary>
    public bool ProfileCompleted { get; set; } = false;

    public DateTime? ProfileCompletedAt { get; set; }

    // ─── Davranış İstatistikleri (SP-01) ──────────────────────────────────
    /// <summary>Son 30 günün ortalama aktif dakikası (sistem hesaplar)</summary>
    public int AverageDailyActiveMinutes { get; set; } = 0;

    /// <summary>Toplam erteleme sayısı (tüm zamanlar)</summary>
    public int TotalPostponeCount { get; set; } = 0;

    /// <summary>En çok atlanan kategori adı</summary>
    public string? MostSkippedCategory { get; set; }

    /// <summary>En verimli olunan sabah saati (ör: 07:00)</summary>
    public TimeOnly? PreferredMorningTime { get; set; }

    // ─── Chatbot & Bildirim Ayarları (SP-01) ──────────────────────────────
    /// <summary>Proaktif nudge bildirimleri aktif mi?</summary>
    public bool NudgeEnabled { get; set; } = true;

    /// <summary>Chatbot kişiliği: friendly/motivator/coach</summary>
    public string ChatPersonality { get; set; } = "friendly";

    /// <summary>Son aktif olma zamanı</summary>
    public DateTime? LastActiveAt { get; set; }

    // ─── SP-04: Tatil / Mola Modu ─────────────────────────────────────────
    /// <summary>Kullanıcı tatil/mola modunda mı? Bu sürede görev gelmez, streak korunur.</summary>
    public bool IsOnVacation { get; set; } = false;

    /// <summary>Tatil/mola başlangıç tarihi</summary>
    public DateOnly? VacationStartDate { get; set; }

    /// <summary>Tatil/mola bitiş tarihi</summary>
    public DateOnly? VacationEndDate { get; set; }

    /// <summary>Tatil nedeni: Tatil / Hastalık / Çok yoğunum</summary>
    public string? VacationReason { get; set; }

    /// <summary>Tatile girmeden önceki streak değeri (tatil sonrası geri yüklenir)</summary>
    public int FrozenStreak { get; set; } = 0;

    // ─── Navigation Properties ─────────────────────────────────────────────
    public ICollection<UserCategoryProgress> CategoryProgresses { get; set; } = default!;
    public ICollection<UserTask> UserTasks { get; set; } = default!;
    public ICollection<UserBadge> UserBadges { get; set; } = default!;
    public ICollection<ChatBotMessage> ChatMessages { get; set; } = default!;

    /// <summary>SP-01: Goals → UserGoalsList olarak rename (isim çakışması düzeltildi)</summary>
    public ICollection<UserGoal> UserGoalsList { get; set; } = default!;

    public ICollection<UserAchievement> UserAchievements { get; set; } = default!;
    public ICollection<Notification> Notifications { get; set; } = default!;
    public ICollection<WeeklyReport> WeeklyReports { get; set; } = default!;
    public ICollection<UserTaskInteraction> TaskInteractions { get; set; } = default!;

    /// <summary>SP-01: Günlük mood kayıtları</summary>
    public ICollection<UserDailyMood> DailyMoods { get; set; } = default!;
}

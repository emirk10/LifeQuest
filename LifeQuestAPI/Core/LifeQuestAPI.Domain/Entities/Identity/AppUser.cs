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
    public bool IsEmailVerified { get; set; } = false;
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpiry { get; set; }

    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    public byte[] PasswordHash { get; set; } = default!;
    public byte[] PasswordSalt { get; set; } = default!;
    public Guid AppRoleId { get; set; }
    public AppRole AppRole { get; set; } = default!;

    // ─── Phase 4: Social & Profile ────────────────────────────────────────
    public string? Bio { get; set; }
    public string? Location { get; set; }
    
    /// <summary>İlgi alanları (ör: ["fitness", "tech", "art"])</summary>
    [Column(TypeName = "jsonb")]
    public List<string>? Interests { get; set; }

    // ─── Gamification ─────────────────────────────────────────────────────
    public int TotalXP { get; set; } = 0;
    public double GeneralLevel { get; set; } = 1.0;

    // MISS-004: Streak sistemi
    public int CurrentStreak { get; set; } = 0;
    public int LongestStreak { get; set; } = 0;
    public DateOnly? LastTaskCompletedDate { get; set; }

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

    // ─── SP-05: Kontrollü Görev Alma Sistemi ──────────────────────────────
    /// <summary>Günde alabileceği maksimum görev sayısı. Varsayılan 3, kullanıcı 1-10 arası değiştirebilir.</summary>
    public int DailyTaskLimit { get; set; } = 3;

    /// <summary>Haftada alabileceği maksimum görev. PreferredTasksPerWeek'ten hesaplanır.</summary>
    public int WeeklyTaskLimit { get; set; } = 7;

    /// <summary>Şu an Selected + InProgress + Paused durumundaki görev sayısı. Her alım/tamamlamada güncellenir.</summary>
    public int ActiveTaskCount { get; set; } = 0;

    /// <summary>Bugün seçilen görev sayısı. Gece 00:00'de DailyQuestWorker tarafından sıfırlanır.</summary>
    public int TodayTaskCount { get; set; } = 0;

    /// <summary>Bu hafta tamamlanan görev sayısı. Pazartesi sabahı sıfırlanır.</summary>
    public int WeeklyCompletedCount { get; set; } = 0;

    /// <summary>Günlük limit dolduğunda uyarının bugün gösterilip gösterilmediği. Gece sıfırlanır.</summary>
    public bool OverLimitWarningShown { get; set; } = false;

    // ─── SP-05: Onboarding Sistemi ────────────────────────────────────────
    /// <summary>Onboarding tamamlandı mı?</summary>
    public bool OnboardingCompleted { get; set; } = false;

    public DateTime? OnboardingCompletedAt { get; set; }

    /// <summary>Onboarding versiyonu — ileride re-onboard için.</summary>
    public int OnboardingVersion { get; set; } = 1;

    /// <summary>Uyanış saati — nudge zamanlaması için.</summary>
    public TimeOnly? WakeUpTime { get; set; }

    /// <summary>Uyku saati — gece mesajlarını engelle.</summary>
    public TimeOnly? SleepTime { get; set; }

    /// <summary>Çalışma günleri jsonb — [1,2,3,4,5] (0=Pazar, 6=Cumartesi).</summary>
    [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "jsonb")]
    public List<int>? WorkDays { get; set; }

    /// <summary>Yaşam tarzı: student/professional/freelancer/homemaker/retired/other</summary>
    public string? LifestyleType { get; set; }

    /// <summary>Yaş aralığı: 18-24/25-34/35-44/45-54/55+</summary>
    public string? AgeRange { get; set; }

    /// <summary>En çok odaklanmak istediği alan: health/career/education/social/mindfulness/finance</summary>
    public string? PrimaryFocusArea { get; set; }

    /// <summary>Motivasyonu öldüren faktörler — jsonb: ["boring_tasks","too_long"]</summary>
    [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "jsonb")]
    public List<string>? MotivationKillers { get; set; }

    /// <summary>Tercih edilen görev süresi: micro_5/short_15/medium_30/long_60/any</summary>
    public string? PreferredTaskDuration { get; set; }

    /// <summary>İlk görev tamamlama deneyimi yaşandı mı?</summary>
    public bool HasCompletedFirstTask { get; set; } = false;

    /// <summary>Seçilen avatar ID'si (1-20 predefined avatar).</summary>
    public int? AvatarId { get; set; }

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

    /// <summary>SP-05: Onboarding ham cevapları</summary>
    public ICollection<OnboardingResponse> OnboardingResponses { get; set; } = default!;
}

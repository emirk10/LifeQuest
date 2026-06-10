using System.ComponentModel.DataAnnotations.Schema;
using LifeQuestAPI.Domain.Entities.Common;

namespace LifeQuestAPI.Domain.Entities.Identity;

public sealed class AppUser : BaseEntity
{
    
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

    
    public string? Bio { get; set; }
    public string? Location { get; set; }
    
    
    [Column(TypeName = "jsonb")]
    public List<string>? Interests { get; set; }

    
    public int TotalXP { get; set; } = 0;
    public double GeneralLevel { get; set; } = 1.0;

    
    public int CurrentStreak { get; set; } = 0;
    public int LongestStreak { get; set; } = 0;
    public DateOnly? LastTaskCompletedDate { get; set; }

    
    
    [Column("UserGoals", TypeName = "jsonb")]
    public List<string>? UserGoals { get; set; }

    
    public int? DailyAvailableMinutes { get; set; }

    
    [Column(TypeName = "jsonb")]
    public List<string>? Strengths { get; set; }

    
    [Column(TypeName = "jsonb")]
    public List<string>? Weaknesses { get; set; }

    
    public string? MotivationFactor { get; set; }

    
    public int? PreferredTasksPerWeek { get; set; }

    
    public string? DifficultyPreference { get; set; }

    
    public bool ProfileCompleted { get; set; } = false;

    public DateTime? ProfileCompletedAt { get; set; }

    
    
    public int AverageDailyActiveMinutes { get; set; } = 0;

    
    public int TotalPostponeCount { get; set; } = 0;

    
    public string? MostSkippedCategory { get; set; }

    
    public TimeOnly? PreferredMorningTime { get; set; }

    
    
    public bool NudgeEnabled { get; set; } = true;

    
    public string ChatPersonality { get; set; } = "friendly";

    
    public DateTime? LastActiveAt { get; set; }

    
    
    public bool IsOnVacation { get; set; } = false;

    
    public DateOnly? VacationStartDate { get; set; }

    
    public DateOnly? VacationEndDate { get; set; }

    
    public string? VacationReason { get; set; }

    
    public int FrozenStreak { get; set; } = 0;

    
    
    public int DailyTaskLimit { get; set; } = 3;

    
    public int WeeklyTaskLimit { get; set; } = 7;

    
    public int ActiveTaskCount { get; set; } = 0;

    
    public int TodayTaskCount { get; set; } = 0;

    
    public int WeeklyCompletedCount { get; set; } = 0;

    
    public bool OverLimitWarningShown { get; set; } = false;

    
    public DateOnly? LastCounterResetDate { get; set; }

    
    public DateOnly? LastWeeklyResetDate { get; set; }

    
    
    public bool OnboardingCompleted { get; set; } = false;

    public DateTime? OnboardingCompletedAt { get; set; }

    
    public int OnboardingVersion { get; set; } = 1;

    
    public TimeOnly? WakeUpTime { get; set; }

    
    public TimeOnly? SleepTime { get; set; }

    
    [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "jsonb")]
    public List<int>? WorkDays { get; set; }

    
    public string? LifestyleType { get; set; }

    
    public string? AgeRange { get; set; }

    
    public string? PrimaryFocusArea { get; set; }

    
    [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "jsonb")]
    public List<string>? MotivationKillers { get; set; }

    
    public string? PreferredTaskDuration { get; set; }

    
    public bool HasCompletedFirstTask { get; set; } = false;

    
    public int? AvatarId { get; set; }

    
    public ICollection<UserCategoryProgress> CategoryProgresses { get; set; } = default!;
    public ICollection<UserTask> UserTasks { get; set; } = default!;
    public ICollection<UserBadge> UserBadges { get; set; } = default!;
    public ICollection<ChatBotMessage> ChatMessages { get; set; } = default!;

    
    public ICollection<UserGoal> UserGoalsList { get; set; } = default!;

    public ICollection<UserAchievement> UserAchievements { get; set; } = default!;
    public ICollection<Notification> Notifications { get; set; } = default!;
    public ICollection<WeeklyReport> WeeklyReports { get; set; } = default!;
    public ICollection<UserTaskInteraction> TaskInteractions { get; set; } = default!;

    
    public ICollection<UserDailyMood> DailyMoods { get; set; } = default!;

    
    public ICollection<OnboardingResponse> OnboardingResponses { get; set; } = default!;
}


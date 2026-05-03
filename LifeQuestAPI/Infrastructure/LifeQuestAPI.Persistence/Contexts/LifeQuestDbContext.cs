using LifeQuestAPI.Domain.Entities.Identity;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LifeQuestAPI.Persistence.Contexts;

public sealed class LifeQuestDbContext : DbContext
{
    public LifeQuestDbContext(DbContextOptions<LifeQuestDbContext> options) : base(options)
    {
    }

    public DbSet<AppUser> AppUsers { get; set; }
    public DbSet<AppRole> AppRoles { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<AppTask> AppTasks { get; set; }
    public DbSet<UserCategoryProgress> UserCategoryProgresses { get; set; }
    public DbSet<UserTask> UserTasks { get; set; }
    public DbSet<Badge> Badges { get; set; }
    public DbSet<UserBadge> UserBadges { get; set; }
    public DbSet<ChatBotMessage> ChatBotMessages { get; set; }
    public DbSet<UserGoal> UserGoals { get; set; }
    public DbSet<Achievement> Achievements { get; set; }
    public DbSet<UserAchievement> UserAchievements { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<WeeklyReport> WeeklyReports { get; set; }
    public DbSet<Friendship> Friendships { get; set; }
    public DbSet<Challenge> Challenges { get; set; }
    public DbSet<UserTaskInteraction> UserTaskInteractions { get; set; }
    public DbSet<UserDailyMood> UserDailyMoods { get; set; }

    // SP-05 Yeni Entity'ler
    public DbSet<OnboardingResponse> OnboardingResponses { get; set; }
    public DbSet<GoalMilestone> GoalMilestones { get; set; }
    public DbSet<ChallengeParticipant> ChallengeParticipants { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserCategoryProgress>()
            .HasIndex(u => new { u.AppUserId, u.CategoryId })
            .IsUnique();

        modelBuilder.Entity<AppUser>()
            .HasOne(u => u.AppRole)
            .WithMany()
            .HasForeignKey(u => u.AppRoleId);

        // Friendship - self-referencing
        modelBuilder.Entity<Friendship>()
            .HasOne(f => f.Requester)
            .WithMany()
            .HasForeignKey(f => f.RequesterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Friendship>()
            .HasOne(f => f.Addressee)
            .WithMany()
            .HasForeignKey(f => f.AddresseeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Friendship>()
            .HasIndex(f => new { f.RequesterId, f.AddresseeId })
            .IsUnique();

        // Challenge - self-referencing
        modelBuilder.Entity<Challenge>()
            .HasOne(c => c.Challenger)
            .WithMany()
            .HasForeignKey(c => c.ChallengerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Challenge>()
            .HasOne(c => c.Opponent)
            .WithMany()
            .HasForeignKey(c => c.OpponentId)
            .OnDelete(DeleteBehavior.Restrict);

        // SP-05: ChallengeParticipants
        modelBuilder.Entity<ChallengeParticipant>()
            .HasOne(cp => cp.Challenge)
            .WithMany(c => c.Participants)
            .HasForeignKey(cp => cp.ChallengeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChallengeParticipant>()
            .HasOne(cp => cp.User)
            .WithMany()
            .HasForeignKey(cp => cp.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // UserAchievement unique constraint
        modelBuilder.Entity<UserAchievement>()
            .HasIndex(ua => new { ua.AppUserId, ua.AchievementId })
            .IsUnique();

        // UserTaskInteraction indexes for ML queries
        modelBuilder.Entity<UserTaskInteraction>()
            .HasIndex(uti => uti.AppUserId);

        modelBuilder.Entity<UserTaskInteraction>()
            .HasIndex(uti => uti.WasCompleted);

        // Notification index
        modelBuilder.Entity<Notification>()
            .HasIndex(n => new { n.AppUserId, n.IsRead });

        // WeeklyReport index
        modelBuilder.Entity<WeeklyReport>()
            .HasIndex(wr => new { wr.AppUserId, wr.WeekStartDate })
            .IsUnique();

        // UserDailyMood — UNIQUE(AppUserId, Date) + indexes
        modelBuilder.Entity<UserDailyMood>()
            .HasIndex(m => new { m.AppUserId, m.Date })
            .IsUnique();

        modelBuilder.Entity<UserDailyMood>()
            .HasIndex(m => m.Date);

        // AppUser jsonb kolonları
        modelBuilder.Entity<AppUser>()
            .Property(u => u.UserGoals)
            .HasColumnType("jsonb")
            .HasColumnName("UserGoals");

        modelBuilder.Entity<AppUser>()
            .Property(u => u.Strengths)
            .HasColumnType("jsonb");

        modelBuilder.Entity<AppUser>()
            .Property(u => u.Weaknesses)
            .HasColumnType("jsonb");

        // AppUser.Goals → UserGoalsList navigation
        modelBuilder.Entity<AppUser>()
            .HasMany(u => u.UserGoalsList)
            .WithOne(g => g.AppUser)
            .HasForeignKey(g => g.AppUserId);

        // AppTask jsonb kolonları
        modelBuilder.Entity<AppTask>()
            .Property(t => t.RecommendedTimeSlots)
            .HasColumnType("jsonb");

        modelBuilder.Entity<AppTask>()
            .Property(t => t.TargetedWeaknesses)
            .HasColumnType("jsonb");

        modelBuilder.Entity<AppTask>()
            .Property(t => t.TargetedGoals)
            .HasColumnType("jsonb");

        modelBuilder.Entity<ChatBotMessage>()
            .HasIndex(m => m.AppUserId);

        modelBuilder.Entity<ChatBotMessage>()
            .Property(m => m.ActionsOfferedJson)
            .HasColumnType("jsonb");

        // Migration Fix: Defaults
        modelBuilder.Entity<AppUser>()
            .Property(u => u.NudgeEnabled)
            .HasDefaultValue(true);

        modelBuilder.Entity<UserTask>()
            .Property(ut => ut.XpCompensationRate)
            .HasDefaultValue(1.0m);

        // SP-05: Task Limit defaults
        modelBuilder.Entity<AppUser>()
            .Property(u => u.DailyTaskLimit)
            .HasDefaultValue(3);

        modelBuilder.Entity<AppUser>()
            .Property(u => u.WeeklyTaskLimit)
            .HasDefaultValue(7);

        // SP-05: AppUser yeni jsonb kolonlari
        modelBuilder.Entity<AppUser>()
            .Property(u => u.WorkDays)
            .HasColumnType("jsonb");

        modelBuilder.Entity<AppUser>()
            .Property(u => u.MotivationKillers)
            .HasColumnType("jsonb");

        // SP-05: OnboardingResponse
        modelBuilder.Entity<OnboardingResponse>()
            .HasOne(o => o.AppUser)
            .WithMany(u => u.OnboardingResponses)
            .HasForeignKey(o => o.AppUserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OnboardingResponse>()
            .HasIndex(o => new { o.AppUserId, o.QuestionKey });

        // SP-05: GoalMilestone
        modelBuilder.Entity<GoalMilestone>()
            .HasOne(gm => gm.UserGoal)
            .WithMany(g => g.Milestones)
            .HasForeignKey(gm => gm.UserGoalId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GoalMilestone>()
            .HasIndex(gm => new { gm.UserGoalId, gm.OrderIndex });

        // SP-05: UserGoal jsonb
        modelBuilder.Entity<UserGoal>()
            .Property(g => g.LinkedCategoryIds)
            .HasColumnType("jsonb");

        // IMP-006: Eksik performans indeksleri
        modelBuilder.Entity<UserTask>()
            .HasIndex(ut => new { ut.AppUserId, ut.Status })
            .HasDatabaseName("IX_UserTask_UserId_Status");

        modelBuilder.Entity<UserTask>()
            .HasIndex(ut => new { ut.AppUserId, ut.DueDate })
            .HasDatabaseName("IX_UserTask_UserId_DueDate");

        modelBuilder.Entity<ChatBotMessage>()
            .HasIndex(m => new { m.AppUserId, m.CreatedAt })
            .HasDatabaseName("IX_ChatBotMessage_UserId_CreatedAt");

        modelBuilder.Entity<UserCategoryProgress>()
            .HasIndex(ucp => ucp.AppUserId)
            .HasDatabaseName("IX_UserCategoryProgress_UserId");

        modelBuilder.Entity<AppTask>()
            .HasIndex(t => new { t.IsActive, t.CategoryId })
            .HasDatabaseName("IX_AppTask_IsActive_CategoryId");

        base.OnModelCreating(modelBuilder);
    }
}
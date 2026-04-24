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

        // ChatBotMessage — SP-02 index
        modelBuilder.Entity<ChatBotMessage>()
            .HasIndex(m => m.AppUserId);

        modelBuilder.Entity<ChatBotMessage>()
            .Property(m => m.ActionsOfferedJson)
            .HasColumnType("jsonb");

        base.OnModelCreating(modelBuilder);
    }
}
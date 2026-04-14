using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LifeQuestAPI.Domain.Entities.Identity;
using LifeQuestAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.Persistence.Contexts;

public sealed class LifeQuestDbContext : DbContext
{
    public LifeQuestDbContext(DbContextOptions<LifeQuestDbContext> options) : base(options)
    {
    }

    // Mevcut DbSet'ler
    public DbSet<AppUser> AppUsers { get; set; }
    public DbSet<AppRole> AppRoles { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<AppTask> AppTasks { get; set; }
    public DbSet<UserCategoryProgress> UserCategoryProgresses { get; set; }
    public DbSet<UserTask> UserTasks { get; set; }
    public DbSet<Badge> Badges { get; set; }
    public DbSet<UserBadge> UserBadges { get; set; }
    public DbSet<ChatBotMessage> ChatBotMessages { get; set; }

    // Yeni DbSet'ler
    public DbSet<TaskVerification> TaskVerifications { get; set; }
    public DbSet<UserGoal> UserGoals { get; set; }
    public DbSet<Achievement> Achievements { get; set; }
    public DbSet<UserAchievement> UserAchievements { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<WeeklyReport> WeeklyReports { get; set; }
    public DbSet<Friendship> Friendships { get; set; }
    public DbSet<Challenge> Challenges { get; set; }
    public DbSet<UserTaskInteraction> UserTaskInteractions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Mevcut konfigürasyonlar
        modelBuilder.Entity<UserCategoryProgress>()
            .HasIndex(u => new { u.AppUserId, u.CategoryId })
            .IsUnique();

        modelBuilder.Entity<AppUser>()
            .HasOne(u => u.AppRole)
            .WithMany()
            .HasForeignKey(u => u.AppRoleId);

        // TaskVerification -> UserTask ilişkisi
        modelBuilder.Entity<TaskVerification>()
            .HasOne(tv => tv.UserTask)
            .WithMany()
            .HasForeignKey(tv => tv.UserTaskId)
            .OnDelete(DeleteBehavior.Cascade);

        // Friendship - self-referencing many-to-many
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

        // Notification index for unread notifications
        modelBuilder.Entity<Notification>()
            .HasIndex(n => new { n.AppUserId, n.IsRead });

        // WeeklyReport index
        modelBuilder.Entity<WeeklyReport>()
            .HasIndex(wr => new { wr.AppUserId, wr.WeekStartDate })
            .IsUnique();

        base.OnModelCreating(modelBuilder);
    }
}
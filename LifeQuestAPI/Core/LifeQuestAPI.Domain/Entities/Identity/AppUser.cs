using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LifeQuestAPI.Domain.Entities.Common;

namespace LifeQuestAPI.Domain.Entities.Identity;

public sealed class AppUser : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string FullName => $"{Name} {Surname}";
    public string UserName { get; set; } = string.Empty ;
    public string Email { get; set; } = string.Empty;

    public byte[] PasswordHash { get; set; } = default!;
    public byte[] PasswordSalt { get; set; } = default!;
    public Guid AppRoleId { get; set; }
    public AppRole AppRole { get; set; } = default!;
    public int TotalXP { get; set; } = 0;
    public double GeneralLevel { get; set; } = 1.0;

    public ICollection<UserCategoryProgress> CategoryProgresses { get; set; } = default!;
    public ICollection<UserTask> UserTasks { get; set; } = default!;
    public ICollection<UserBadge> UserBadges { get; set; } = default!;
    public ICollection<ChatBotMessage> ChatMessages { get; set; } = default!;

    // Yeni navigation property'ler
    public ICollection<UserGoal> Goals { get; set; } = default!;
    public ICollection<UserAchievement> UserAchievements { get; set; } = default!;
    public ICollection<Notification> Notifications { get; set; } = default!;
    public ICollection<WeeklyReport> WeeklyReports { get; set; } = default!;
    public ICollection<UserTaskInteraction> TaskInteractions { get; set; } = default!;
}
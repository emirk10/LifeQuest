using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeQuestAPI.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InfrastructureAndPerformance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserTask_UserId_DueDate",
                table: "UserTasks");

            migrationBuilder.DropIndex(
                name: "IX_UserTask_UserId_Status",
                table: "UserTasks");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_AppUserId_IsRead",
                table: "Notifications");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "WeeklyReports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "WeeklyReports",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "UserTasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "UserTasks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "UserTaskInteractions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "UserTaskInteractions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "UserGoals",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "UserGoals",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "UserDailyMoods",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "UserDailyMoods",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "UserCategoryProgresses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "UserCategoryProgresses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "UserBadges",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "UserBadges",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "UserAchievements",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "UserAchievements",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "PostTags",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "PostTags",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Posts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Posts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "PostLikes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "PostLikes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "PostComments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "PostComments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "OnboardingResponses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "OnboardingResponses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Notifications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Notifications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "GoalMilestones",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "GoalMilestones",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Friendships",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Friendships",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ChatBotMessages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ChatBotMessages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Challenges",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Challenges",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ChallengeParticipants",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ChallengeParticipants",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Categories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Categories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Badges",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Badges",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AppUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AppUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AppTasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AppTasks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AppRoles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AppRoles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Achievements",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Achievements",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_UserTask_UserId_DueDate_CreatedAt",
                table: "UserTasks",
                columns: new[] { "AppUserId", "DueDate", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserTask_UserId_Status_CreatedAt",
                table: "UserTasks",
                columns: new[] { "AppUserId", "Status", "CreatedAt" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserTask_XpRate",
                table: "UserTasks",
                sql: "\"XpCompensationRate\" >= 0 AND \"XpCompensationRate\" <= 1.5");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_UserId_CreatedAt",
                table: "Notifications",
                columns: new[] { "AppUserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Notification_UserId_IsRead_CreatedAt",
                table: "Notifications",
                columns: new[] { "AppUserId", "IsRead", "CreatedAt" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_AppTask_BaseXP",
                table: "AppTasks",
                sql: "\"BaseXP\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AppTask_Levels",
                table: "AppTasks",
                sql: "\"MaxLevel\" >= \"MinLevel\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AppTask_MinLevel",
                table: "AppTasks",
                sql: "\"MinLevel\" >= 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserTask_UserId_DueDate_CreatedAt",
                table: "UserTasks");

            migrationBuilder.DropIndex(
                name: "IX_UserTask_UserId_Status_CreatedAt",
                table: "UserTasks");

            migrationBuilder.DropCheckConstraint(
                name: "CK_UserTask_XpRate",
                table: "UserTasks");

            migrationBuilder.DropIndex(
                name: "IX_Notification_UserId_CreatedAt",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notification_UserId_IsRead_CreatedAt",
                table: "Notifications");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AppTask_BaseXP",
                table: "AppTasks");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AppTask_Levels",
                table: "AppTasks");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AppTask_MinLevel",
                table: "AppTasks");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "WeeklyReports");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "WeeklyReports");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "UserTasks");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "UserTasks");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "UserTaskInteractions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "UserTaskInteractions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "UserGoals");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "UserGoals");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "UserDailyMoods");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "UserDailyMoods");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "UserCategoryProgresses");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "UserCategoryProgresses");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "UserBadges");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "UserBadges");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "UserAchievements");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "UserAchievements");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "PostTags");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "PostTags");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "PostLikes");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "PostLikes");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "PostComments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "PostComments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "OnboardingResponses");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "OnboardingResponses");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "GoalMilestones");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "GoalMilestones");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Friendships");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Friendships");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ChatBotMessages");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ChatBotMessages");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ChallengeParticipants");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ChallengeParticipants");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Badges");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Badges");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AppTasks");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AppTasks");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AppRoles");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AppRoles");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Achievements");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Achievements");

            migrationBuilder.CreateIndex(
                name: "IX_UserTask_UserId_DueDate",
                table: "UserTasks",
                columns: new[] { "AppUserId", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_UserTask_UserId_Status",
                table: "UserTasks",
                columns: new[] { "AppUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_AppUserId_IsRead",
                table: "Notifications",
                columns: new[] { "AppUserId", "IsRead" });
        }
    }
}

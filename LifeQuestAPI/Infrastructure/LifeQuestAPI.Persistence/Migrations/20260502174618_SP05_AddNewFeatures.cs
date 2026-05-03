using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeQuestAPI.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SP05_AddNewFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WinnerBonusXP",
                table: "Challenges",
                newName: "WinnerXPBonus");

            migrationBuilder.RenameColumn(
                name: "TargetTaskCount",
                table: "Challenges",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Challenges",
                newName: "IsPublic");

            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAt",
                table: "UserGoals",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AutoTrackByCategory",
                table: "UserGoals",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AutoTrackByXP",
                table: "UserGoals",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "UserGoals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "UserGoals",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompletionBadgeId",
                table: "UserGoals",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompletionXP",
                table: "UserGoals",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "UserGoals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "UserGoals",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastReminderAt",
                table: "UserGoals",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<List<Guid>>(
                name: "LinkedCategoryIds",
                table: "UserGoals",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProgressUnit",
                table: "UserGoals",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "ReminderEnabled",
                table: "UserGoals",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ReminderFrequency",
                table: "UserGoals",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Why",
                table: "UserGoals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptedAt",
                table: "Challenges",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChallengeType",
                table: "Challenges",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ChallengerMessage",
                table: "Challenges",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Challenges",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InvitedAt",
                table: "Challenges",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "LoserXPPenalty",
                table: "Challenges",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "TargetAppTaskId",
                table: "Challenges",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TargetCategoryId",
                table: "Challenges",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetValue",
                table: "Challenges",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ActiveTaskCount",
                table: "AppUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AgeRange",
                table: "AppUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AvatarId",
                table: "AppUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DailyTaskLimit",
                table: "AppUsers",
                type: "integer",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.AddColumn<bool>(
                name: "HasCompletedFirstTask",
                table: "AppUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LifestyleType",
                table: "AppUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "MotivationKillers",
                table: "AppUsers",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OnboardingCompleted",
                table: "AppUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "OnboardingCompletedAt",
                table: "AppUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OnboardingVersion",
                table: "AppUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "OverLimitWarningShown",
                table: "AppUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PreferredTaskDuration",
                table: "AppUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryFocusArea",
                table: "AppUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "SleepTime",
                table: "AppUsers",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TodayTaskCount",
                table: "AppUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "WakeUpTime",
                table: "AppUsers",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WeeklyCompletedCount",
                table: "AppUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WeeklyTaskLimit",
                table: "AppUsers",
                type: "integer",
                nullable: false,
                defaultValue: 7);

            migrationBuilder.AddColumn<List<int>>(
                name: "WorkDays",
                table: "AppUsers",
                type: "jsonb",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ChallengeParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChallengeId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    CurrentProgress = table.Column<int>(type: "integer", nullable: false),
                    IsWinner = table.Column<bool>(type: "boolean", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChallengeParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChallengeParticipants_AppUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChallengeParticipants_Challenges_ChallengeId",
                        column: x => x.ChallengeId,
                        principalTable: "Challenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GoalMilestones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserGoalId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    TargetValue = table.Column<int>(type: "integer", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BonusXP = table.Column<int>(type: "integer", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoalMilestones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoalMilestones_UserGoals_UserGoalId",
                        column: x => x.UserGoalId,
                        principalTable: "UserGoals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OnboardingResponses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionKey = table.Column<string>(type: "text", nullable: false),
                    Answer = table.Column<string>(type: "text", nullable: false),
                    AnsweredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnboardingResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnboardingResponses_AppUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Challenges_TargetAppTaskId",
                table: "Challenges",
                column: "TargetAppTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Challenges_TargetCategoryId",
                table: "Challenges",
                column: "TargetCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeParticipants_ChallengeId",
                table: "ChallengeParticipants",
                column: "ChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeParticipants_UserId",
                table: "ChallengeParticipants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GoalMilestones_UserGoalId_OrderIndex",
                table: "GoalMilestones",
                columns: new[] { "UserGoalId", "OrderIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingResponses_AppUserId_QuestionKey",
                table: "OnboardingResponses",
                columns: new[] { "AppUserId", "QuestionKey" });

            migrationBuilder.AddForeignKey(
                name: "FK_Challenges_AppTasks_TargetAppTaskId",
                table: "Challenges",
                column: "TargetAppTaskId",
                principalTable: "AppTasks",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Challenges_Categories_TargetCategoryId",
                table: "Challenges",
                column: "TargetCategoryId",
                principalTable: "Categories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Challenges_AppTasks_TargetAppTaskId",
                table: "Challenges");

            migrationBuilder.DropForeignKey(
                name: "FK_Challenges_Categories_TargetCategoryId",
                table: "Challenges");

            migrationBuilder.DropTable(
                name: "ChallengeParticipants");

            migrationBuilder.DropTable(
                name: "GoalMilestones");

            migrationBuilder.DropTable(
                name: "OnboardingResponses");

            migrationBuilder.DropIndex(
                name: "IX_Challenges_TargetAppTaskId",
                table: "Challenges");

            migrationBuilder.DropIndex(
                name: "IX_Challenges_TargetCategoryId",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "UserGoals");

            migrationBuilder.DropColumn(
                name: "AutoTrackByCategory",
                table: "UserGoals");

            migrationBuilder.DropColumn(
                name: "AutoTrackByXP",
                table: "UserGoals");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "UserGoals");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "UserGoals");

            migrationBuilder.DropColumn(
                name: "CompletionBadgeId",
                table: "UserGoals");

            migrationBuilder.DropColumn(
                name: "CompletionXP",
                table: "UserGoals");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "UserGoals");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "UserGoals");

            migrationBuilder.DropColumn(
                name: "LastReminderAt",
                table: "UserGoals");

            migrationBuilder.DropColumn(
                name: "LinkedCategoryIds",
                table: "UserGoals");

            migrationBuilder.DropColumn(
                name: "ProgressUnit",
                table: "UserGoals");

            migrationBuilder.DropColumn(
                name: "ReminderEnabled",
                table: "UserGoals");

            migrationBuilder.DropColumn(
                name: "ReminderFrequency",
                table: "UserGoals");

            migrationBuilder.DropColumn(
                name: "Why",
                table: "UserGoals");

            migrationBuilder.DropColumn(
                name: "AcceptedAt",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "ChallengeType",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "ChallengerMessage",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "InvitedAt",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "LoserXPPenalty",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "TargetAppTaskId",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "TargetCategoryId",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "TargetValue",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "ActiveTaskCount",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "AgeRange",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "AvatarId",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "DailyTaskLimit",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "HasCompletedFirstTask",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "LifestyleType",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "MotivationKillers",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "OnboardingCompleted",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "OnboardingCompletedAt",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "OnboardingVersion",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "OverLimitWarningShown",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "PreferredTaskDuration",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "PrimaryFocusArea",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "SleepTime",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "TodayTaskCount",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "WakeUpTime",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "WeeklyCompletedCount",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "WeeklyTaskLimit",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "WorkDays",
                table: "AppUsers");

            migrationBuilder.RenameColumn(
                name: "WinnerXPBonus",
                table: "Challenges",
                newName: "WinnerBonusXP");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Challenges",
                newName: "TargetTaskCount");

            migrationBuilder.RenameColumn(
                name: "IsPublic",
                table: "Challenges",
                newName: "IsActive");
        }
    }
}

using System;
using System.Collections.Generic;
using LifeQuestAPI.Domain.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeQuestAPI.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SP01_MoodSystemAndTaskEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompensatedAt",
                table: "UserTasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DayTimeSlot",
                table: "UserTasks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompensated",
                table: "UserTasks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPostponed",
                table: "UserTasks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PausedAt",
                table: "UserTasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PostponeCount",
                table: "UserTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "PostponedTo",
                table: "UserTasks",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SkipReason",
                table: "UserTasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalPausedMinutes",
                table: "UserTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UserEnergyAtStart",
                table: "UserTasks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserMoodAtStart",
                table: "UserTasks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "XpCompensationRate",
                table: "UserTasks",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "AverageDailyActiveMinutes",
                table: "AppUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ChatPersonality",
                table: "AppUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DailyAvailableMinutes",
                table: "AppUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DifficultyPreference",
                table: "AppUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastActiveAt",
                table: "AppUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MostSkippedCategory",
                table: "AppUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivationFactor",
                table: "AppUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NudgeEnabled",
                table: "AppUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "PreferredMorningTime",
                table: "AppUsers",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreferredTasksPerWeek",
                table: "AppUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ProfileCompleted",
                table: "AppUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProfileCompletedAt",
                table: "AppUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "Strengths",
                table: "AppUsers",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalPostponeCount",
                table: "AppUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<List<string>>(
                name: "UserGoals",
                table: "AppUsers",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "Weaknesses",
                table: "AppUsers",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BadgeHint",
                table: "AppTasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChunkMinutes",
                table: "AppTasks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstimatedMinutes",
                table: "AppTasks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsChunkable",
                table: "AppTasks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxEnergyLevel",
                table: "AppTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinEnergyLevel",
                table: "AppTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<List<DayTimeSlot>>(
                name: "RecommendedTimeSlots",
                table: "AppTasks",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "TargetedGoals",
                table: "AppTasks",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "TargetedWeaknesses",
                table: "AppTasks",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhyThisTask",
                table: "AppTasks",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserDailyMoods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    MoodMode = table.Column<int>(type: "integer", nullable: false),
                    AvailableMinutes = table.Column<int>(type: "integer", nullable: false),
                    EnergyLevel = table.Column<int>(type: "integer", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDailyMoods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDailyMoods_AppUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserDailyMoods_AppUserId_Date",
                table: "UserDailyMoods",
                columns: new[] { "AppUserId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDailyMoods_Date",
                table: "UserDailyMoods",
                column: "Date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserDailyMoods");

            migrationBuilder.DropColumn(
                name: "CompensatedAt",
                table: "UserTasks");

            migrationBuilder.DropColumn(
                name: "DayTimeSlot",
                table: "UserTasks");

            migrationBuilder.DropColumn(
                name: "IsCompensated",
                table: "UserTasks");

            migrationBuilder.DropColumn(
                name: "IsPostponed",
                table: "UserTasks");

            migrationBuilder.DropColumn(
                name: "PausedAt",
                table: "UserTasks");

            migrationBuilder.DropColumn(
                name: "PostponeCount",
                table: "UserTasks");

            migrationBuilder.DropColumn(
                name: "PostponedTo",
                table: "UserTasks");

            migrationBuilder.DropColumn(
                name: "SkipReason",
                table: "UserTasks");

            migrationBuilder.DropColumn(
                name: "TotalPausedMinutes",
                table: "UserTasks");

            migrationBuilder.DropColumn(
                name: "UserEnergyAtStart",
                table: "UserTasks");

            migrationBuilder.DropColumn(
                name: "UserMoodAtStart",
                table: "UserTasks");

            migrationBuilder.DropColumn(
                name: "XpCompensationRate",
                table: "UserTasks");

            migrationBuilder.DropColumn(
                name: "AverageDailyActiveMinutes",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "ChatPersonality",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "DailyAvailableMinutes",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "DifficultyPreference",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "LastActiveAt",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "MostSkippedCategory",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "MotivationFactor",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "NudgeEnabled",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "PreferredMorningTime",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "PreferredTasksPerWeek",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "ProfileCompleted",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "ProfileCompletedAt",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "Strengths",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "TotalPostponeCount",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "UserGoals",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "Weaknesses",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "BadgeHint",
                table: "AppTasks");

            migrationBuilder.DropColumn(
                name: "ChunkMinutes",
                table: "AppTasks");

            migrationBuilder.DropColumn(
                name: "EstimatedMinutes",
                table: "AppTasks");

            migrationBuilder.DropColumn(
                name: "IsChunkable",
                table: "AppTasks");

            migrationBuilder.DropColumn(
                name: "MaxEnergyLevel",
                table: "AppTasks");

            migrationBuilder.DropColumn(
                name: "MinEnergyLevel",
                table: "AppTasks");

            migrationBuilder.DropColumn(
                name: "RecommendedTimeSlots",
                table: "AppTasks");

            migrationBuilder.DropColumn(
                name: "TargetedGoals",
                table: "AppTasks");

            migrationBuilder.DropColumn(
                name: "TargetedWeaknesses",
                table: "AppTasks");

            migrationBuilder.DropColumn(
                name: "WhyThisTask",
                table: "AppTasks");
        }
    }
}

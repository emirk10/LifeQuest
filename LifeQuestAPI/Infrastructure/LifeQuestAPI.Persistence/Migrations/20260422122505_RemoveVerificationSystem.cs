using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeQuestAPI.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveVerificationSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskVerifications");

            migrationBuilder.DropColumn(
                name: "ExpectedImageCategory",
                table: "AppTasks");

            migrationBuilder.DropColumn(
                name: "MinReflectionWords",
                table: "AppTasks");

            migrationBuilder.DropColumn(
                name: "MinTimerMinutes",
                table: "AppTasks");

            migrationBuilder.DropColumn(
                name: "VerificationHint",
                table: "AppTasks");

            migrationBuilder.DropColumn(
                name: "VerificationType",
                table: "AppTasks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExpectedImageCategory",
                table: "AppTasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinReflectionWords",
                table: "AppTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinTimerMinutes",
                table: "AppTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "VerificationHint",
                table: "AppTasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VerificationType",
                table: "AppTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TaskVerifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    AIAnalysisResult = table.Column<string>(type: "text", nullable: true),
                    AIConfidenceScore = table.Column<float>(type: "real", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsReflectionRelevant = table.Column<bool>(type: "boolean", nullable: true),
                    MinimumMinutesRequired = table.Column<int>(type: "integer", nullable: true),
                    PhotoPath = table.Column<string>(type: "text", nullable: true),
                    QuizScore = table.Column<int>(type: "integer", nullable: true),
                    QuizTotalQuestions = table.Column<int>(type: "integer", nullable: true),
                    ReflectionText = table.Column<string>(type: "text", nullable: true),
                    ReflectionWordCount = table.Column<int>(type: "integer", nullable: true),
                    ReviewNotes = table.Column<string>(type: "text", nullable: true),
                    SentimentScore = table.Column<float>(type: "real", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TimerEndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TimerStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskVerifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskVerifications_UserTasks_UserTaskId",
                        column: x => x.UserTaskId,
                        principalTable: "UserTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskVerifications_UserTaskId",
                table: "TaskVerifications",
                column: "UserTaskId");
        }
    }
}

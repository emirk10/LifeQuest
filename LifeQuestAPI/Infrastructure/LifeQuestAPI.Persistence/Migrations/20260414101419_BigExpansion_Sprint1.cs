using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeQuestAPI.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BigExpansion_Sprint1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "Achievements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IconPath = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    RequiredCount = table.Column<int>(type: "integer", nullable: false),
                    BonusXP = table.Column<int>(type: "integer", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achievements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Challenges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChallengerId = table.Column<Guid>(type: "uuid", nullable: false),
                    OpponentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: false),
                    TargetTaskCount = table.Column<int>(type: "integer", nullable: false),
                    ChallengerProgress = table.Column<int>(type: "integer", nullable: false),
                    OpponentProgress = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WinnerId = table.Column<Guid>(type: "uuid", nullable: true),
                    WinnerBonusXP = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Challenges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Challenges_AppUsers_ChallengerId",
                        column: x => x.ChallengerId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Challenges_AppUsers_OpponentId",
                        column: x => x.OpponentId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Challenges_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Friendships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequesterId = table.Column<Guid>(type: "uuid", nullable: false),
                    AddresseeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Friendships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Friendships_AppUsers_AddresseeId",
                        column: x => x.AddresseeId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Friendships_AppUsers_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActionUrl = table.Column<string>(type: "text", nullable: true),
                    IconEmoji = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_AppUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskVerifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PhotoPath = table.Column<string>(type: "text", nullable: true),
                    AIConfidenceScore = table.Column<float>(type: "real", nullable: true),
                    AIAnalysisResult = table.Column<string>(type: "text", nullable: true),
                    ReflectionText = table.Column<string>(type: "text", nullable: true),
                    ReflectionWordCount = table.Column<int>(type: "integer", nullable: true),
                    SentimentScore = table.Column<float>(type: "real", nullable: true),
                    IsReflectionRelevant = table.Column<bool>(type: "boolean", nullable: true),
                    TimerStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TimerEndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MinimumMinutesRequired = table.Column<int>(type: "integer", nullable: true),
                    QuizScore = table.Column<int>(type: "integer", nullable: true),
                    QuizTotalQuestions = table.Column<int>(type: "integer", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewNotes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "UserGoals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    TargetValue = table.Column<int>(type: "integer", nullable: false),
                    CurrentValue = table.Column<int>(type: "integer", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    BonusXP = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGoals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserGoals_AppUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserGoals_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserTaskInteractions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AppTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserLevel = table.Column<int>(type: "integer", nullable: false),
                    UserCategoryLevel = table.Column<int>(type: "integer", nullable: false),
                    TaskDifficulty = table.Column<int>(type: "integer", nullable: false),
                    TaskBaseXP = table.Column<int>(type: "integer", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    HourOfDay = table.Column<int>(type: "integer", nullable: false),
                    UserCompletedTaskCount = table.Column<int>(type: "integer", nullable: false),
                    UserStreakDays = table.Column<int>(type: "integer", nullable: false),
                    WasCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CompletionTimeHours = table.Column<float>(type: "real", nullable: false),
                    Rating = table.Column<float>(type: "real", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTaskInteractions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTaskInteractions_AppTasks_AppTaskId",
                        column: x => x.AppTaskId,
                        principalTable: "AppTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserTaskInteractions_AppUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeeklyReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    WeekStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WeekEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TasksCompleted = table.Column<int>(type: "integer", nullable: false),
                    TasksFailed = table.Column<int>(type: "integer", nullable: false),
                    TotalXPEarned = table.Column<int>(type: "integer", nullable: false),
                    StreakDays = table.Column<int>(type: "integer", nullable: false),
                    StrongestCategory = table.Column<string>(type: "text", nullable: false),
                    WeakestCategory = table.Column<string>(type: "text", nullable: false),
                    AIInsight = table.Column<string>(type: "text", nullable: true),
                    AIRecommendation = table.Column<string>(type: "text", nullable: true),
                    OverallPerformanceScore = table.Column<float>(type: "real", nullable: true),
                    CategoryBreakdownJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeeklyReports_AppUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAchievements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AchievementId = table.Column<Guid>(type: "uuid", nullable: false),
                    EarnedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CurrentProgress = table.Column<int>(type: "integer", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAchievements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAchievements_Achievements_AchievementId",
                        column: x => x.AchievementId,
                        principalTable: "Achievements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAchievements_AppUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Challenges_CategoryId",
                table: "Challenges",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Challenges_ChallengerId",
                table: "Challenges",
                column: "ChallengerId");

            migrationBuilder.CreateIndex(
                name: "IX_Challenges_OpponentId",
                table: "Challenges",
                column: "OpponentId");

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_AddresseeId",
                table: "Friendships",
                column: "AddresseeId");

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_RequesterId_AddresseeId",
                table: "Friendships",
                columns: new[] { "RequesterId", "AddresseeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_AppUserId_IsRead",
                table: "Notifications",
                columns: new[] { "AppUserId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskVerifications_UserTaskId",
                table: "TaskVerifications",
                column: "UserTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievements_AchievementId",
                table: "UserAchievements",
                column: "AchievementId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievements_AppUserId_AchievementId",
                table: "UserAchievements",
                columns: new[] { "AppUserId", "AchievementId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserGoals_AppUserId",
                table: "UserGoals",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGoals_CategoryId",
                table: "UserGoals",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTaskInteractions_AppTaskId",
                table: "UserTaskInteractions",
                column: "AppTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTaskInteractions_AppUserId",
                table: "UserTaskInteractions",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTaskInteractions_WasCompleted",
                table: "UserTaskInteractions",
                column: "WasCompleted");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyReports_AppUserId_WeekStartDate",
                table: "WeeklyReports",
                columns: new[] { "AppUserId", "WeekStartDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Challenges");

            migrationBuilder.DropTable(
                name: "Friendships");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "TaskVerifications");

            migrationBuilder.DropTable(
                name: "UserAchievements");

            migrationBuilder.DropTable(
                name: "UserGoals");

            migrationBuilder.DropTable(
                name: "UserTaskInteractions");

            migrationBuilder.DropTable(
                name: "WeeklyReports");

            migrationBuilder.DropTable(
                name: "Achievements");

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
    }
}

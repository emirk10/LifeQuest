using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeQuestAPI.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserTasks_AppUserId",
                table: "UserTasks");

            migrationBuilder.CreateIndex(
                name: "IX_UserTask_UserId_DueDate",
                table: "UserTasks",
                columns: new[] { "AppUserId", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_UserTask_UserId_Status",
                table: "UserTasks",
                columns: new[] { "AppUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_UserCategoryProgress_UserId",
                table: "UserCategoryProgresses",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatBotMessage_UserId_CreatedAt",
                table: "ChatBotMessages",
                columns: new[] { "AppUserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AppTask_IsActive_CategoryId",
                table: "AppTasks",
                columns: new[] { "IsActive", "CategoryId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserTask_UserId_DueDate",
                table: "UserTasks");

            migrationBuilder.DropIndex(
                name: "IX_UserTask_UserId_Status",
                table: "UserTasks");

            migrationBuilder.DropIndex(
                name: "IX_UserCategoryProgress_UserId",
                table: "UserCategoryProgresses");

            migrationBuilder.DropIndex(
                name: "IX_ChatBotMessage_UserId_CreatedAt",
                table: "ChatBotMessages");

            migrationBuilder.DropIndex(
                name: "IX_AppTask_IsActive_CategoryId",
                table: "AppTasks");

            migrationBuilder.CreateIndex(
                name: "IX_UserTasks_AppUserId",
                table: "UserTasks",
                column: "AppUserId");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeQuestAPI.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTaskIdToPostTag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserTaskId",
                table: "PostTags",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PostTags_UserTaskId",
                table: "PostTags",
                column: "UserTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_PostTags_UserTasks_UserTaskId",
                table: "PostTags",
                column: "UserTaskId",
                principalTable: "UserTasks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostTags_UserTasks_UserTaskId",
                table: "PostTags");

            migrationBuilder.DropIndex(
                name: "IX_PostTags_UserTaskId",
                table: "PostTags");

            migrationBuilder.DropColumn(
                name: "UserTaskId",
                table: "PostTags");
        }
    }
}

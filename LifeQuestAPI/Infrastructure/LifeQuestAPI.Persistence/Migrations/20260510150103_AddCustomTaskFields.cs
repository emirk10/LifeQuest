using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeQuestAPI.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomTaskFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JournalEntry",
                table: "UserDailyMoods",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SleepQuality",
                table: "UserDailyMoods",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "AppTasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCustom",
                table: "AppTasks",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JournalEntry",
                table: "UserDailyMoods");

            migrationBuilder.DropColumn(
                name: "SleepQuality",
                table: "UserDailyMoods");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "AppTasks");

            migrationBuilder.DropColumn(
                name: "IsCustom",
                table: "AppTasks");
        }
    }
}

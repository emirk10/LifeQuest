using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeQuestAPI.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SP04_VacationAndInteractions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FrozenStreak",
                table: "AppUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsOnVacation",
                table: "AppUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "VacationEndDate",
                table: "AppUsers",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VacationReason",
                table: "AppUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "VacationStartDate",
                table: "AppUsers",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FrozenStreak",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "IsOnVacation",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "VacationEndDate",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "VacationReason",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "VacationStartDate",
                table: "AppUsers");
        }
    }
}

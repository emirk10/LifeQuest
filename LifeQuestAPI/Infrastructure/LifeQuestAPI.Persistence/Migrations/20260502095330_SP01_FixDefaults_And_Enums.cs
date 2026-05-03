using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeQuestAPI.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SP01_FixDefaults_And_Enums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "XpCompensationRate",
                table: "UserTasks",
                type: "numeric",
                nullable: false,
                defaultValue: 1.0m,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<bool>(
                name: "NudgeEnabled",
                table: "AppUsers",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "XpCompensationRate",
                table: "UserTasks",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldDefaultValue: 1.0m);

            migrationBuilder.AlterColumn<bool>(
                name: "NudgeEnabled",
                table: "AppUsers",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);
        }
    }
}

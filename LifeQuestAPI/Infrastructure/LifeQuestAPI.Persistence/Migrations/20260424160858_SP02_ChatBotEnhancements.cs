using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeQuestAPI.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SP02_ChatBotEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActionSelected",
                table: "ChatBotMessages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActionTaken",
                table: "ChatBotMessages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "ActionsOfferedJson",
                table: "ChatBotMessages",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Intent",
                table: "ChatBotMessages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "IntentConfidence",
                table: "ChatBotMessages",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsProactive",
                table: "ChatBotMessages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "RelatedTaskId",
                table: "ChatBotMessages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sentiment",
                table: "ChatBotMessages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "SentimentConfidence",
                table: "ChatBotMessages",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UrgencyLevel",
                table: "ChatBotMessages",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActionSelected",
                table: "ChatBotMessages");

            migrationBuilder.DropColumn(
                name: "ActionTaken",
                table: "ChatBotMessages");

            migrationBuilder.DropColumn(
                name: "ActionsOfferedJson",
                table: "ChatBotMessages");

            migrationBuilder.DropColumn(
                name: "Intent",
                table: "ChatBotMessages");

            migrationBuilder.DropColumn(
                name: "IntentConfidence",
                table: "ChatBotMessages");

            migrationBuilder.DropColumn(
                name: "IsProactive",
                table: "ChatBotMessages");

            migrationBuilder.DropColumn(
                name: "RelatedTaskId",
                table: "ChatBotMessages");

            migrationBuilder.DropColumn(
                name: "Sentiment",
                table: "ChatBotMessages");

            migrationBuilder.DropColumn(
                name: "SentimentConfidence",
                table: "ChatBotMessages");

            migrationBuilder.DropColumn(
                name: "UrgencyLevel",
                table: "ChatBotMessages");
        }
    }
}

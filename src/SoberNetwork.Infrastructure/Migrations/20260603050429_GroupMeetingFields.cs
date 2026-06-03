using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoberNetwork.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GroupMeetingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                schema: "public",
                table: "groups",
                type: "integer",
                nullable: false,
                defaultValue: 60);

            migrationBuilder.AddColumn<bool>(
                name: "IsOpen",
                schema: "public",
                table: "groups",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                schema: "public",
                table: "groups",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Language",
                schema: "public",
                table: "groups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MeetingDay",
                schema: "public",
                table: "groups",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MeetingFormats",
                schema: "public",
                table: "groups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MeetingTime",
                schema: "public",
                table: "groups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresApproval",
                schema: "public",
                table: "groups",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "ZoomMeetingId",
                schema: "public",
                table: "groups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZoomPasscode",
                schema: "public",
                table: "groups",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                schema: "public",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "IsOpen",
                schema: "public",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                schema: "public",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "Language",
                schema: "public",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "MeetingDay",
                schema: "public",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "MeetingFormats",
                schema: "public",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "MeetingTime",
                schema: "public",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "RequiresApproval",
                schema: "public",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "ZoomMeetingId",
                schema: "public",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "ZoomPasscode",
                schema: "public",
                table: "groups");
        }
    }
}

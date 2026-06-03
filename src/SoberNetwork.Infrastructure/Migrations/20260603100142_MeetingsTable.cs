using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoberNetwork.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MeetingsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "meetings",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsRecurring = table.Column<bool>(type: "boolean", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: true),
                    Time = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 60),
                    OccursOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsOpen = table.Column<bool>(type: "boolean", nullable: false),
                    Formats = table.Column<string>(type: "text", nullable: true),
                    Language = table.Column<string>(type: "text", nullable: true),
                    Location = table.Column<string>(type: "text", nullable: true),
                    ZoomLink = table.Column<string>(type: "text", nullable: true),
                    ZoomMeetingId = table.Column<string>(type: "text", nullable: true),
                    ZoomPasscode = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meetings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_meetings_groups_GroupId",
                        column: x => x.GroupId,
                        principalSchema: "public",
                        principalTable: "groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_meetings_GroupId_IsActive_DeletedAt",
                schema: "public",
                table: "meetings",
                columns: new[] { "GroupId", "IsActive", "DeletedAt" });

            // Backfill: migrate existing group meeting data into the new meetings table.
            // One meeting row is created per group that has a MeetingTime or MeetingDay set.
            // Platform has only test data at this point — this preserves any existing meeting configuration.
            migrationBuilder.Sql(@"
                INSERT INTO public.meetings (
                    ""Id"", ""GroupId"", ""Name"", ""IsRecurring"", ""DayOfWeek"", ""Time"",
                    ""DurationMinutes"", ""IsOpen"", ""Formats"", ""Language"",
                    ""ZoomLink"", ""ZoomMeetingId"", ""ZoomPasscode"",
                    ""IsActive"", ""CreatedAt"", ""UpdatedAt""
                )
                SELECT
                    gen_random_uuid(),
                    g.""Id"",
                    COALESCE(NULLIF(TRIM(g.""MeetingSchedule""), ''), g.""Name"" || ' Meeting'),
                    true,
                    g.""MeetingDay"",
                    COALESCE(NULLIF(TRIM(g.""MeetingTime""), ''), '00:00'),
                    g.""DurationMinutes"",
                    g.""IsOpen"",
                    g.""MeetingFormats"",
                    g.""Language"",
                    g.""ZoomLink"",
                    g.""ZoomMeetingId"",
                    g.""ZoomPasscode"",
                    true,
                    g.""CreatedAt"",
                    NOW()
                FROM public.groups g
                WHERE g.""DeletedAt"" IS NULL
                  AND (g.""MeetingTime"" IS NOT NULL OR g.""MeetingDay"" IS NOT NULL);
            ");

            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                schema: "public",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "IsOpen",
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
                name: "MeetingSchedule",
                schema: "public",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "MeetingTime",
                schema: "public",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "ZoomLink",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "meetings",
                schema: "public");

            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                schema: "public",
                table: "groups",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsOpen",
                schema: "public",
                table: "groups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

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
                name: "MeetingSchedule",
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

            migrationBuilder.AddColumn<string>(
                name: "ZoomLink",
                schema: "public",
                table: "groups",
                type: "text",
                nullable: true);

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
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoberNetwork.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MigrateToMultipleDaysOfWeek : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_meetings_IsActive_DeletedAt_MeetingType_DayOfWeek",
                schema: "public",
                table: "meetings");

            migrationBuilder.DropColumn(
                name: "DayOfWeek",
                schema: "public",
                table: "meetings");

            migrationBuilder.AddColumn<int[]>(
                name: "DaysOfWeek",
                schema: "public",
                table: "meetings",
                type: "integer[]",
                nullable: true,
                defaultValueSql: "ARRAY[]::integer[]");

            migrationBuilder.CreateIndex(
                name: "IX_meetings_IsActive_DeletedAt_MeetingType",
                schema: "public",
                table: "meetings",
                columns: new[] { "IsActive", "DeletedAt", "MeetingType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_meetings_IsActive_DeletedAt_MeetingType",
                schema: "public",
                table: "meetings");

            migrationBuilder.DropColumn(
                name: "DaysOfWeek",
                schema: "public",
                table: "meetings");

            migrationBuilder.AddColumn<int?>(
                name: "DayOfWeek",
                schema: "public",
                table: "meetings",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_meetings_IsActive_DeletedAt_MeetingType_DayOfWeek",
                schema: "public",
                table: "meetings",
                columns: new[] { "IsActive", "DeletedAt", "MeetingType", "DayOfWeek" });
        }
    }
}

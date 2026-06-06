using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoberNetwork.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDaysOfWeekColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop old DayOfWeek column if it exists
            migrationBuilder.Sql("ALTER TABLE public.meetings DROP COLUMN IF EXISTS \"DayOfWeek\" CASCADE;");

            // Add new DaysOfWeek column
            migrationBuilder.AddColumn<int[]>(
                name: "DaysOfWeek",
                schema: "public",
                table: "meetings",
                type: "integer[]",
                nullable: true);

            // Set default value using SQL
            migrationBuilder.Sql("ALTER TABLE public.meetings ALTER COLUMN \"DaysOfWeek\" SET DEFAULT ARRAY[]::integer[];");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaysOfWeek",
                schema: "public",
                table: "meetings");
        }
    }
}

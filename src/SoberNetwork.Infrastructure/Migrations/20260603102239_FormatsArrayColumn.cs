using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoberNetwork.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FormatsArrayColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the old text column and re-add as text[].
            // Platform has only test data at this point — no data migration required.
            migrationBuilder.DropColumn(
                name: "Formats",
                schema: "public",
                table: "meetings");

            migrationBuilder.AddColumn<List<string>>(
                name: "Formats",
                schema: "public",
                table: "meetings",
                type: "text[]",
                nullable: false,
                defaultValueSql: "ARRAY[]::text[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Formats",
                schema: "public",
                table: "meetings");

            migrationBuilder.AddColumn<string>(
                name: "Formats",
                schema: "public",
                table: "meetings",
                type: "text",
                nullable: true);
        }
    }
}

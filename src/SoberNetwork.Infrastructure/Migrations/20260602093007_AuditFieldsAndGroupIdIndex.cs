using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoberNetwork.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AuditFieldsAndGroupIdIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "public",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "public",
                table: "users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "public",
                table: "groups",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "public",
                table: "groups",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "public",
                table: "group_memberships",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "public",
                table: "group_memberships",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "public",
                table: "users");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "public",
                table: "users");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "public",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "public",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "public",
                table: "group_memberships");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "public",
                table: "group_memberships");
        }
    }
}

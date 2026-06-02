using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoberNetwork.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MembersApiSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDaysSoberPublic",
                schema: "public",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TimeZone",
                schema: "public",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPhoneShared",
                schema: "public",
                table: "group_memberships",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDaysSoberPublic",
                schema: "public",
                table: "users");

            migrationBuilder.DropColumn(
                name: "TimeZone",
                schema: "public",
                table: "users");

            migrationBuilder.DropColumn(
                name: "IsPhoneShared",
                schema: "public",
                table: "group_memberships");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoberNetwork.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesForQueryColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_CreatedAt",
                schema: "public",
                table: "refresh_tokens",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_meetings_CreatedAt",
                schema: "public",
                table: "meetings",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_groups_CreatedAt",
                schema: "public",
                table: "groups",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_group_memberships_CreatedAt",
                schema: "public",
                table: "group_memberships",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_group_memberships_GroupId_Status_DeletedAt",
                schema: "public",
                table: "group_memberships",
                columns: new[] { "GroupId", "Status", "DeletedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_refresh_tokens_CreatedAt",
                schema: "public",
                table: "refresh_tokens");

            migrationBuilder.DropIndex(
                name: "IX_meetings_CreatedAt",
                schema: "public",
                table: "meetings");

            migrationBuilder.DropIndex(
                name: "IX_groups_CreatedAt",
                schema: "public",
                table: "groups");

            migrationBuilder.DropIndex(
                name: "IX_group_memberships_CreatedAt",
                schema: "public",
                table: "group_memberships");

            migrationBuilder.DropIndex(
                name: "IX_group_memberships_GroupId_Status_DeletedAt",
                schema: "public",
                table: "group_memberships");
        }
    }
}

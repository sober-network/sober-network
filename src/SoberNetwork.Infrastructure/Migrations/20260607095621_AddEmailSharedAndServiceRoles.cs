using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoberNetwork.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailSharedAndServiceRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEmailShared",
                schema: "public",
                table: "group_memberships",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "group_service_roles",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleType = table.Column<string>(type: "text", nullable: false),
                    CustomTitle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_group_service_roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_group_service_roles_groups_GroupId",
                        column: x => x.GroupId,
                        principalSchema: "public",
                        principalTable: "groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_group_service_roles_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_group_service_roles_GroupId",
                schema: "public",
                table: "group_service_roles",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_group_service_roles_GroupId_RoleType",
                schema: "public",
                table: "group_service_roles",
                columns: new[] { "GroupId", "RoleType" });

            migrationBuilder.CreateIndex(
                name: "IX_group_service_roles_GroupId_UserId",
                schema: "public",
                table: "group_service_roles",
                columns: new[] { "GroupId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_group_service_roles_UserId",
                schema: "public",
                table: "group_service_roles",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "group_service_roles",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "IsEmailShared",
                schema: "public",
                table: "group_memberships");
        }
    }
}

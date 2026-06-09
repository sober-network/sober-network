using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoberNetwork.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                schema: "public",
                table: "post_comments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinkTitle",
                schema: "public",
                table: "post_comments",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinkUrl",
                schema: "public",
                table: "post_comments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                schema: "public",
                table: "post_comments");

            migrationBuilder.DropColumn(
                name: "LinkTitle",
                schema: "public",
                table: "post_comments");

            migrationBuilder.DropColumn(
                name: "LinkUrl",
                schema: "public",
                table: "post_comments");
        }
    }
}

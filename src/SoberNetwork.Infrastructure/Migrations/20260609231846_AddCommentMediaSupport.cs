using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoberNetwork.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentMediaSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CommentId",
                schema: "public",
                table: "post_media",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MediaId",
                schema: "public",
                table: "post_comments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_post_media_CommentId",
                schema: "public",
                table: "post_media",
                column: "CommentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_post_media_post_comments_CommentId",
                schema: "public",
                table: "post_media",
                column: "CommentId",
                principalSchema: "public",
                principalTable: "post_comments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_post_media_post_comments_CommentId",
                schema: "public",
                table: "post_media");

            migrationBuilder.DropIndex(
                name: "IX_post_media_CommentId",
                schema: "public",
                table: "post_media");

            migrationBuilder.DropColumn(
                name: "CommentId",
                schema: "public",
                table: "post_media");

            migrationBuilder.DropColumn(
                name: "MediaId",
                schema: "public",
                table: "post_comments");
        }
    }
}

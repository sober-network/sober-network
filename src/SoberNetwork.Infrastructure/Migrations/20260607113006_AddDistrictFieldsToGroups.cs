using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoberNetwork.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDistrictFieldsToGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AreaName",
                schema: "public",
                table: "groups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AreaWebsiteUrl",
                schema: "public",
                table: "groups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DistrictLatitude",
                schema: "public",
                table: "groups",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DistrictLongitude",
                schema: "public",
                table: "groups",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DistrictName",
                schema: "public",
                table: "groups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DistrictWebsiteUrl",
                schema: "public",
                table: "groups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                schema: "public",
                table: "groups",
                type: "text",
                nullable: true);

            // Seed district data for Rock Hill, SC groups
            migrationBuilder.Sql(@"
                UPDATE public.groups
                SET 
                    ""DistrictName"" = 'District 5',
                    ""AreaName"" = 'Area 1',
                    ""State"" = 'South Carolina',
                    ""DistrictWebsiteUrl"" = 'https://aa-district5sc.org/',
                    ""AreaWebsiteUrl"" = 'https://scarea1aa.org/',
                    ""DistrictLatitude"" = 34.9256,
                    ""DistrictLongitude"" = -81.0287,
                    ""UpdatedAt"" = NOW()
                WHERE ""Slug"" = 'early-bird-zoom'
                   OR ""Slug"" = 'earlybird'
                   OR ""Slug"" LIKE '%rock%hill%'
                   OR ""Slug"" LIKE '%early%bird%';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AreaName",
                schema: "public",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "AreaWebsiteUrl",
                schema: "public",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "DistrictLatitude",
                schema: "public",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "DistrictLongitude",
                schema: "public",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "DistrictName",
                schema: "public",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "DistrictWebsiteUrl",
                schema: "public",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "State",
                schema: "public",
                table: "groups");
        }
    }
}

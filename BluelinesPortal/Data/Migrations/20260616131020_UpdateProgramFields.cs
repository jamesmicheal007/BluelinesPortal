using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BluelinesPortal.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProgramFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BrochurePath",
                table: "Programs",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Prerequisites",
                table: "Programs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShortDescription",
                table: "Programs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailPath",
                table: "Programs",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "YouTubeVideoUrl",
                table: "Programs",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BrochurePath",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "Prerequisites",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "ShortDescription",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "ThumbnailPath",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "YouTubeVideoUrl",
                table: "Programs");
        }
    }
}

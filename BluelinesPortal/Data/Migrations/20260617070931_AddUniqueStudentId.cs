using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BluelinesPortal.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueStudentId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StudentId",
                table: "StudentProfiles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "StudentProfiles");
        }
    }
}

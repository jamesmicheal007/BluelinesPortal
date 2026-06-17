using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BluelinesPortal.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectSubmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Submissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentApplicationId = table.Column<int>(type: "int", nullable: false),
                    SubmissionTitle = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    GitHubRepositoryUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CloudDriveLink = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StudentNotes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MentorFeedback = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReviewStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SubmittedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Submissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Submissions_Applications_StudentApplicationId",
                        column: x => x.StudentApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_StudentApplicationId",
                table: "Submissions",
                column: "StudentApplicationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Submissions");
        }
    }
}

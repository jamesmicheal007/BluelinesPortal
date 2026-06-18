using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BluelinesPortal.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdvancedLmsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CouponCode",
                table: "Programs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiscountType",
                table: "Programs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountValue",
                table: "Programs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsDiscountActive",
                table: "Programs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "VideoUrl",
                table: "Lessons",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Lessons",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "DownloadableAssetPath",
                table: "Lessons",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PdfDocumentPath",
                table: "Lessons",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalFee",
                table: "Applications",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "ProjectMaterials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramItemId = table.Column<int>(type: "int", nullable: false),
                    AssetType = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExternalUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsPremium = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectMaterials_Programs_ProgramItemId",
                        column: x => x.ProgramItemId,
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMaterials_ProgramItemId",
                table: "ProjectMaterials",
                column: "ProgramItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectMaterials");

            migrationBuilder.DropColumn(
                name: "CouponCode",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "DiscountType",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "DiscountValue",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "IsDiscountActive",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "DownloadableAssetPath",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "PdfDocumentPath",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "FinalFee",
                table: "Applications");

            migrationBuilder.AlterColumn<string>(
                name: "VideoUrl",
                table: "Lessons",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Lessons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}

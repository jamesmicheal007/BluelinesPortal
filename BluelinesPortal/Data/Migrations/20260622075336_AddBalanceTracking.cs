using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BluelinesPortal.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBalanceTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BalanceScreenshotPath",
                table: "ProductOrders",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BalanceStatus",
                table: "ProductOrders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BalanceUTRNumber",
                table: "ProductOrders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BalanceScreenshotPath",
                table: "ProductOrders");

            migrationBuilder.DropColumn(
                name: "BalanceStatus",
                table: "ProductOrders");

            migrationBuilder.DropColumn(
                name: "BalanceUTRNumber",
                table: "ProductOrders");
        }
    }
}

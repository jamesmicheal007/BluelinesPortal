using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BluelinesPortal.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdvancedEcommerce : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) 
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AddOnTotal",
                table: "ProductOrders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BalanceDue",
                table: "ProductOrders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsSplitPayment",
                table: "ProductOrders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SelectedAddOns",
                table: "ProductOrders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicableFor",
                table: "DigitalProducts",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryTime",
                table: "DigitalProducts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Modules",
                table: "DigitalProducts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OriginalPrice",
                table: "DigitalProducts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddOnTotal",
                table: "ProductOrders");

            migrationBuilder.DropColumn(
                name: "BalanceDue",
                table: "ProductOrders");

            migrationBuilder.DropColumn(
                name: "IsSplitPayment",
                table: "ProductOrders");

            migrationBuilder.DropColumn(
                name: "SelectedAddOns",
                table: "ProductOrders");

            migrationBuilder.DropColumn(
                name: "ApplicableFor",
                table: "DigitalProducts");

            migrationBuilder.DropColumn(
                name: "DeliveryTime",
                table: "DigitalProducts");

            migrationBuilder.DropColumn(
                name: "Modules",
                table: "DigitalProducts");

            migrationBuilder.DropColumn(
                name: "OriginalPrice",
                table: "DigitalProducts");
        }
    }
}

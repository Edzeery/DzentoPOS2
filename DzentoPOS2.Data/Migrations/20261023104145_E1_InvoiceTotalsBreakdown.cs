using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DzentoPOS2.Data.Migrations
{
    /// <inheritdoc />
    public partial class E1_InvoiceTotalsBreakdown : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "SalesInvoices",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "DiscountCurrency",
                table: "SalesInvoices",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "SubtotalAmount",
                table: "SalesInvoices",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SubtotalCurrency",
                table: "SalesInvoices",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                table: "SalesInvoices",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TaxCurrency",
                table: "SalesInvoices",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "DiscountCurrency",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "SubtotalAmount",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "SubtotalCurrency",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "TaxCurrency",
                table: "SalesInvoices");
        }
    }
}

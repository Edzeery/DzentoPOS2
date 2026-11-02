using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DzentoPOS2.Data.Migrations
{
    /// <inheritdoc />
    public partial class ProfitRebuild_Phase1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FeesAmount",
                table: "SalesInvoices",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "FeesCurrency",
                table: "SalesInvoices",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "SalesInvoiceItems",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsCostEstimated",
                table: "SalesInvoiceItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ListPrice",
                table: "SalesInvoiceItems",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitCost",
                table: "SalesInvoiceItems",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            // Backfill legacy data: freeze the current product cost as an estimate,
            // and use the historical line price as the list price.
            migrationBuilder.Sql(
                "UPDATE SalesInvoiceItems SET " +
                "UnitCost = COALESCE((SELECT CostPriceAmount FROM Products WHERE Products.Id = SalesInvoiceItems.ProductId), 0), " +
                "ListPrice = Price, " +
                "DiscountAmount = 0, " +
                "IsCostEstimated = 1;");

            migrationBuilder.Sql(
                "UPDATE SalesInvoices SET FeesCurrency = COALESCE(NULLIF(TotalCurrency, ''), 'DZD'), FeesAmount = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FeesAmount",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "FeesCurrency",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "SalesInvoiceItems");

            migrationBuilder.DropColumn(
                name: "IsCostEstimated",
                table: "SalesInvoiceItems");

            migrationBuilder.DropColumn(
                name: "ListPrice",
                table: "SalesInvoiceItems");

            migrationBuilder.DropColumn(
                name: "UnitCost",
                table: "SalesInvoiceItems");
        }
    }
}

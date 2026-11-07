using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DzentoPOS2.Data.Migrations
{
    /// <inheritdoc />
    public partial class PhaseC_IndexesAndAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SalesReturns_CreatedAt",
                table: "SalesReturns",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturns_CreatedBy_CreatedAt",
                table: "SalesReturns",
                columns: new[] { "CreatedBy", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoices_CreatedAt",
                table: "SalesInvoices",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoices_CreatedBy_CreatedAt",
                table: "SalesInvoices",
                columns: new[] { "CreatedBy", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoices_CreatedAt",
                table: "PurchaseInvoices",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoices_CreatedBy_CreatedAt",
                table: "PurchaseInvoices",
                columns: new[] { "CreatedBy", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_CreatedAt",
                table: "Expenses",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_CreatedBy_CreatedAt",
                table: "Expenses",
                columns: new[] { "CreatedBy", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SalesReturns_CreatedAt",
                table: "SalesReturns");

            migrationBuilder.DropIndex(
                name: "IX_SalesReturns_CreatedBy_CreatedAt",
                table: "SalesReturns");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoices_CreatedAt",
                table: "SalesInvoices");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoices_CreatedBy_CreatedAt",
                table: "SalesInvoices");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseInvoices_CreatedAt",
                table: "PurchaseInvoices");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseInvoices_CreatedBy_CreatedAt",
                table: "PurchaseInvoices");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_CreatedAt",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_CreatedBy_CreatedAt",
                table: "Expenses");
        }
    }
}

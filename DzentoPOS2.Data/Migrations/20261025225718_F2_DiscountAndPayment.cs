using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DzentoPOS2.Data.Migrations
{
    /// <inheritdoc />
    public partial class F2_DiscountAndPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ChangeAmount",
                table: "SalesPayments",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TenderedAmount",
                table: "SalesPayments",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "DiscountMode",
                table: "SalesInvoices",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountValue",
                table: "SalesInvoices",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChangeAmount",
                table: "SalesPayments");

            migrationBuilder.DropColumn(
                name: "TenderedAmount",
                table: "SalesPayments");

            migrationBuilder.DropColumn(
                name: "DiscountMode",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "DiscountValue",
                table: "SalesInvoices");
        }
    }
}

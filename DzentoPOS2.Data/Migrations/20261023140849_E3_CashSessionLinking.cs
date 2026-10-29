using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DzentoPOS2.Data.Migrations
{
    /// <inheritdoc />
    public partial class E3_CashSessionLinking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_CashTransactions_CashSessions_CashSessionId",
                table: "CashTransactions",
                column: "CashSessionId",
                principalTable: "CashSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoices_CashSessions_CashSessionId",
                table: "SalesInvoices",
                column: "CashSessionId",
                principalTable: "CashSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CashTransactions_CashSessions_CashSessionId",
                table: "CashTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoices_CashSessions_CashSessionId",
                table: "SalesInvoices");
        }
    }
}

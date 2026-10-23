using System;
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
            migrationBuilder.DropIndex(
                name: "IX_CashSessions_CashboxId",
                table: "CashSessions");

            migrationBuilder.AddColumn<Guid>(
                name: "CashSessionId",
                table: "SalesInvoices",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CashSessionId",
                table: "CashTransactions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceId",
                table: "CashTransactions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceType",
                table: "CashTransactions",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoices_CashSessionId",
                table: "SalesInvoices",
                column: "CashSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_CashTransactions_CashSessionId",
                table: "CashTransactions",
                column: "CashSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_CashTransactions_SourceType_SourceId",
                table: "CashTransactions",
                columns: new[] { "SourceType", "SourceId" });

            migrationBuilder.CreateIndex(
                name: "UX_CashSessions_OpenPerCashbox",
                table: "CashSessions",
                column: "CashboxId",
                unique: true,
                filter: "\"Status\" = 0 AND \"IsDeleted\" = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SalesInvoices_CashSessionId",
                table: "SalesInvoices");

            migrationBuilder.DropIndex(
                name: "IX_CashTransactions_CashSessionId",
                table: "CashTransactions");

            migrationBuilder.DropIndex(
                name: "IX_CashTransactions_SourceType_SourceId",
                table: "CashTransactions");

            migrationBuilder.DropIndex(
                name: "UX_CashSessions_OpenPerCashbox",
                table: "CashSessions");

            migrationBuilder.DropColumn(
                name: "CashSessionId",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "CashSessionId",
                table: "CashTransactions");

            migrationBuilder.DropColumn(
                name: "SourceId",
                table: "CashTransactions");

            migrationBuilder.DropColumn(
                name: "SourceType",
                table: "CashTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_CashSessions_CashboxId",
                table: "CashSessions",
                column: "CashboxId");
        }
    }
}

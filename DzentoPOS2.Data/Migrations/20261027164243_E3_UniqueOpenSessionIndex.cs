using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DzentoPOS2.Data.Migrations
{
    /// <inheritdoc />
    public partial class E3_UniqueOpenSessionIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CashSessions_CashboxId",
                table: "CashSessions");

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
                name: "UX_CashSessions_OpenPerCashbox",
                table: "CashSessions");

            migrationBuilder.CreateIndex(
                name: "IX_CashSessions_CashboxId",
                table: "CashSessions",
                column: "CashboxId");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DzentoPOS2.Data.Migrations
{
    /// <inheritdoc />
    public partial class E2_InventoryAndVoid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_InventoryTransactions_ProductId\";");

            migrationBuilder.AddColumn<bool>(
                name: "IsVoided",
                table: "SalesInvoices",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "VoidReason",
                table: "SalesInvoices",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VoidedAtUtc",
                table: "SalesInvoices",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VoidedByUserId",
                table: "SalesInvoices",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "ProductStocks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "QuantityAfter",
                table: "InventoryTransactions",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "QuantityBefore",
                table: "InventoryTransactions",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceId",
                table: "InventoryTransactions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceType",
                table: "InventoryTransactions",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "InventoryTransactions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoices_IsVoided",
                table: "SalesInvoices",
                column: "IsVoided");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_ProductId_WarehouseId_CreatedAt",
                table: "InventoryTransactions",
                columns: new[] { "ProductId", "WarehouseId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_SourceType_SourceId",
                table: "InventoryTransactions",
                columns: new[] { "SourceType", "SourceId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SalesInvoices_IsVoided",
                table: "SalesInvoices");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransactions_ProductId_WarehouseId_CreatedAt",
                table: "InventoryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransactions_SourceType_SourceId",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "IsVoided",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "VoidReason",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "VoidedAtUtc",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "VoidedByUserId",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "ProductStocks");

            migrationBuilder.DropColumn(
                name: "QuantityAfter",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "QuantityBefore",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "SourceId",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "SourceType",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "InventoryTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_ProductId",
                table: "InventoryTransactions",
                column: "ProductId");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DzentoPOS2.Data.Migrations
{
    /// <inheritdoc />
    public partial class SerialUnitAndWarranty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IMEI",
                table: "SalesInvoiceItems",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                table: "SalesInvoiceItems",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WarrantyExpiry",
                table: "SalesInvoiceItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WarrantyMonths",
                table: "ProductVariants",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SerialUnits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    VariantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SerialNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Imei1 = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Imei2 = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    PurchaseInvoiceItemId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SalesInvoiceItemId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UnitCost = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SoldAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    WarrantyStartsAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    WarrantyExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SerialUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SerialUnits_ProductVariants_VariantId",
                        column: x => x.VariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SerialUnits_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalesInvoiceItemUnits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SalesInvoiceItemId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SerialUnitId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesInvoiceItemUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesInvoiceItemUnits_SalesInvoiceItems_SalesInvoiceItemId",
                        column: x => x.SalesInvoiceItemId,
                        principalTable: "SalesInvoiceItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesInvoiceItemUnits_SerialUnits_SerialUnitId",
                        column: x => x.SerialUnitId,
                        principalTable: "SerialUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceItemUnits_SalesInvoiceItemId_SerialUnitId",
                table: "SalesInvoiceItemUnits",
                columns: new[] { "SalesInvoiceItemId", "SerialUnitId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceItemUnits_SerialUnitId",
                table: "SalesInvoiceItemUnits",
                column: "SerialUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_SerialUnits_Imei1",
                table: "SerialUnits",
                column: "Imei1",
                unique: true,
                filter: "[Imei1] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SerialUnits_Status",
                table: "SerialUnits",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SerialUnits_VariantId_SerialNumber",
                table: "SerialUnits",
                columns: new[] { "VariantId", "SerialNumber" },
                unique: true,
                filter: "[SerialNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SerialUnits_WarehouseId",
                table: "SerialUnits",
                column: "WarehouseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesInvoiceItemUnits");

            migrationBuilder.DropTable(
                name: "SerialUnits");

            migrationBuilder.DropColumn(
                name: "IMEI",
                table: "SalesInvoiceItems");

            migrationBuilder.DropColumn(
                name: "SerialNumber",
                table: "SalesInvoiceItems");

            migrationBuilder.DropColumn(
                name: "WarrantyExpiry",
                table: "SalesInvoiceItems");

            migrationBuilder.DropColumn(
                name: "WarrantyMonths",
                table: "ProductVariants");
        }
    }
}

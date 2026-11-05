using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DzentoPOS2.Data.Migrations
{
    /// <inheritdoc />
    public partial class CatalogPhase1_DataModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductBarcodes_Products_ProductId",
                table: "ProductBarcodes");

            migrationBuilder.DropIndex(
                name: "IX_ProductStocks_ProductId_WarehouseId",
                table: "ProductStocks");

            migrationBuilder.DropIndex(
                name: "IX_Products_Barcode",
                table: "Products");

            migrationBuilder.AddColumn<Guid>(
                name: "VariantId",
                table: "SalesInvoiceItems",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "VariantId",
                table: "PurchaseInvoiceItems",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "CostPriceAmount",
                table: "ProductVariants",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CostPriceCurrency",
                table: "ProductVariants",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "ProductVariants",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ProductVariants",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "ProductVariants",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MinimumStock",
                table: "ProductVariants",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SKU",
                table: "ProductVariants",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SalePriceAmount",
                table: "ProductVariants",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SalePriceCurrency",
                table: "ProductVariants",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailPath",
                table: "ProductVariants",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TrackStock",
                table: "ProductVariants",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "VariantId",
                table: "ProductStocks",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "Products",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Products",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductKind",
                table: "Products",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailPath",
                table: "Products",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                table: "ProductBarcodes",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddColumn<Guid>(
                name: "VariantId",
                table: "ProductBarcodes",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "VariantId",
                table: "InventoryTransactions",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // --- Backfill default variants + VariantId (Phase 1) ---
            migrationBuilder.Sql(@"
INSERT INTO ProductVariants (Id, ProductId, Name, AdditionalPrice, CostPriceAmount, CostPriceCurrency, SalePriceAmount, SalePriceCurrency, MinimumStock, TrackStock, IsActive, IsDefault, CreatedAt, IsDeleted)
SELECT
  (substr(hex(randomblob(16)),1,8) || '-' || substr(hex(randomblob(16)),9,4) || '-' || substr(hex(randomblob(16)),13,4) || '-' || substr(hex(randomblob(16)),17,4) || '-' || substr(hex(randomblob(16)),21,12)),
  p.Id, p.Name, 0, p.CostPriceAmount, p.CostPriceCurrency, p.SalePriceAmount, p.SalePriceCurrency, p.MinimumStock, p.TrackStock, p.IsActive, 1, datetime('now'), 0
FROM Products p
WHERE NOT EXISTS (SELECT 1 FROM ProductVariants v WHERE v.ProductId = p.Id AND v.IsDefault = 1);
");

            migrationBuilder.Sql(@"
UPDATE ProductStocks
SET VariantId = (SELECT v.Id FROM ProductVariants v WHERE v.ProductId = ProductStocks.ProductId AND v.IsDefault = 1 LIMIT 1)
WHERE VariantId = '00000000-0000-0000-0000-000000000000';
");

            migrationBuilder.Sql(@"
UPDATE InventoryTransactions
SET VariantId = (SELECT v.Id FROM ProductVariants v WHERE v.ProductId = InventoryTransactions.ProductId AND v.IsDefault = 1 LIMIT 1)
WHERE VariantId = '00000000-0000-0000-0000-000000000000';
");

            migrationBuilder.Sql(@"
UPDATE SalesInvoiceItems
SET VariantId = (SELECT v.Id FROM ProductVariants v WHERE v.ProductId = SalesInvoiceItems.ProductId AND v.IsDefault = 1 LIMIT 1)
WHERE VariantId = '00000000-0000-0000-0000-000000000000';
");

            migrationBuilder.Sql(@"
UPDATE PurchaseInvoiceItems
SET VariantId = (SELECT v.Id FROM ProductVariants v WHERE v.ProductId = PurchaseInvoiceItems.ProductId AND v.IsDefault = 1 LIMIT 1)
WHERE VariantId = '00000000-0000-0000-0000-000000000000';
");

            migrationBuilder.Sql(@"
UPDATE ProductBarcodes
SET VariantId = (SELECT v.Id FROM ProductVariants v WHERE v.ProductId = ProductBarcodes.ProductId AND v.IsDefault = 1 LIMIT 1)
WHERE VariantId = '00000000-0000-0000-0000-000000000000' AND ProductId IS NOT NULL;
");

            migrationBuilder.Sql(@"
INSERT INTO ProductBarcodes (Id, ProductId, VariantId, Barcode, IsPrimary, CreatedAt, IsDeleted)
SELECT
  (substr(hex(randomblob(16)),1,8) || '-' || substr(hex(randomblob(16)),9,4) || '-' || substr(hex(randomblob(16)),13,4) || '-' || substr(hex(randomblob(16)),17,4) || '-' || substr(hex(randomblob(16)),21,12)),
  p.Id,
  (SELECT v.Id FROM ProductVariants v WHERE v.ProductId = p.Id AND v.IsDefault = 1 LIMIT 1),
  p.Barcode, 1, datetime('now'), 0
FROM Products p
WHERE p.Barcode IS NOT NULL AND p.Barcode <> ''
  AND NOT EXISTS (SELECT 1 FROM ProductBarcodes b WHERE b.Barcode = p.Barcode);
");

            migrationBuilder.CreateTable(
                name: "AttributeDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttributeDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttributeDefinitions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AttributeValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DefinitionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttributeValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttributeValues_AttributeDefinitions_DefinitionId",
                        column: x => x.DefinitionId,
                        principalTable: "AttributeDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VariantAttributes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    VariantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DefinitionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ValueId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VariantAttributes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VariantAttributes_AttributeDefinitions_DefinitionId",
                        column: x => x.DefinitionId,
                        principalTable: "AttributeDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VariantAttributes_AttributeValues_ValueId",
                        column: x => x.ValueId,
                        principalTable: "AttributeValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VariantAttributes_ProductVariants_VariantId",
                        column: x => x.VariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceItems_VariantId",
                table: "SalesInvoiceItems",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceItems_VariantId",
                table: "PurchaseInvoiceItems",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_SKU",
                table: "ProductVariants",
                column: "SKU",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductStocks_ProductId_VariantId_WarehouseId",
                table: "ProductStocks",
                columns: new[] { "ProductId", "VariantId", "WarehouseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductStocks_VariantId",
                table: "ProductStocks",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Barcode",
                table: "Products",
                column: "Barcode");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBarcodes_VariantId",
                table: "ProductBarcodes",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_VariantId",
                table: "InventoryTransactions",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_AttributeDefinitions_ProductId_Name",
                table: "AttributeDefinitions",
                columns: new[] { "ProductId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttributeValues_DefinitionId_Value",
                table: "AttributeValues",
                columns: new[] { "DefinitionId", "Value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VariantAttributes_DefinitionId",
                table: "VariantAttributes",
                column: "DefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_VariantAttributes_ValueId",
                table: "VariantAttributes",
                column: "ValueId");

            migrationBuilder.CreateIndex(
                name: "IX_VariantAttributes_VariantId_DefinitionId",
                table: "VariantAttributes",
                columns: new[] { "VariantId", "DefinitionId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransactions_ProductVariants_VariantId",
                table: "InventoryTransactions",
                column: "VariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductBarcodes_ProductVariants_VariantId",
                table: "ProductBarcodes",
                column: "VariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductBarcodes_Products_ProductId",
                table: "ProductBarcodes",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductStocks_ProductVariants_VariantId",
                table: "ProductStocks",
                column: "VariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransactions_ProductVariants_VariantId",
                table: "InventoryTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductBarcodes_ProductVariants_VariantId",
                table: "ProductBarcodes");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductBarcodes_Products_ProductId",
                table: "ProductBarcodes");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductStocks_ProductVariants_VariantId",
                table: "ProductStocks");

            migrationBuilder.DropTable(
                name: "VariantAttributes");

            migrationBuilder.DropTable(
                name: "AttributeValues");

            migrationBuilder.DropTable(
                name: "AttributeDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoiceItems_VariantId",
                table: "SalesInvoiceItems");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseInvoiceItems_VariantId",
                table: "PurchaseInvoiceItems");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_SKU",
                table: "ProductVariants");

            migrationBuilder.DropIndex(
                name: "IX_ProductStocks_ProductId_VariantId_WarehouseId",
                table: "ProductStocks");

            migrationBuilder.DropIndex(
                name: "IX_ProductStocks_VariantId",
                table: "ProductStocks");

            migrationBuilder.DropIndex(
                name: "IX_Products_Barcode",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_ProductBarcodes_VariantId",
                table: "ProductBarcodes");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransactions_VariantId",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "VariantId",
                table: "SalesInvoiceItems");

            migrationBuilder.DropColumn(
                name: "VariantId",
                table: "PurchaseInvoiceItems");

            migrationBuilder.DropColumn(
                name: "CostPriceAmount",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "CostPriceCurrency",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "MinimumStock",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "SKU",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "SalePriceAmount",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "SalePriceCurrency",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "ThumbnailPath",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "TrackStock",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "VariantId",
                table: "ProductStocks");

            migrationBuilder.DropColumn(
                name: "Brand",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductKind",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ThumbnailPath",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "VariantId",
                table: "ProductBarcodes");

            migrationBuilder.DropColumn(
                name: "VariantId",
                table: "InventoryTransactions");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductId",
                table: "ProductBarcodes",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductStocks_ProductId_WarehouseId",
                table: "ProductStocks",
                columns: new[] { "ProductId", "WarehouseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Barcode",
                table: "Products",
                column: "Barcode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductBarcodes_Products_ProductId",
                table: "ProductBarcodes",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

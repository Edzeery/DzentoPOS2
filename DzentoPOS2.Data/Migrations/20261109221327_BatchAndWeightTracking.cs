using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DzentoPOS2.Data.Migrations
{
    /// <inheritdoc />
    public partial class BatchAndWeightTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BatchId",
                table: "SerialUnits",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsWeighed",
                table: "ProductVariants",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TrackBatch",
                table: "ProductVariants",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TrackExpiry",
                table: "ProductVariants",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "WeightUnitId",
                table: "ProductVariants",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Batches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    VariantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "TEXT", nullable: false),
                    BatchNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Quantity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false),
                    UnitCost = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Batches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Batches_ProductVariants_VariantId",
                        column: x => x.VariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Batches_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SerialUnits_BatchId",
                table: "SerialUnits",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Batches_ExpiryDate",
                table: "Batches",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_Batches_VariantId",
                table: "Batches",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_Batches_WarehouseId",
                table: "Batches",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_SerialUnits_Batches_BatchId",
                table: "SerialUnits",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SerialUnits_Batches_BatchId",
                table: "SerialUnits");

            migrationBuilder.DropTable(
                name: "Batches");

            migrationBuilder.DropIndex(
                name: "IX_SerialUnits_BatchId",
                table: "SerialUnits");

            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "SerialUnits");

            migrationBuilder.DropColumn(
                name: "IsWeighed",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "TrackBatch",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "TrackExpiry",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "WeightUnitId",
                table: "ProductVariants");
        }
    }
}

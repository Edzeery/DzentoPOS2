using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DzentoPOS2.Data.Migrations
{
    /// <inheritdoc />
    public partial class PhaseInventorySnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InventorySnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SnapshotDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TotalQuantity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    CostValue = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    RetailValue = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ExpectedProfit = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    AtRiskProfit = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    DiscountFactorAvg = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    FeeFactor = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventorySnapshots", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventorySnapshots_SnapshotDate",
                table: "InventorySnapshots",
                column: "SnapshotDate",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventorySnapshots");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DzentoPOS2.Data.Migrations
{
    /// <inheritdoc />
    public partial class InventorySnapshotBranchId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InventorySnapshots_SnapshotDate",
                table: "InventorySnapshots");

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "InventorySnapshots",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventorySnapshots_BranchId_SnapshotDate",
                table: "InventorySnapshots",
                columns: new[] { "BranchId", "SnapshotDate" },
                unique: true,
                filter: "[BranchId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InventorySnapshots_SnapshotDate",
                table: "InventorySnapshots",
                column: "SnapshotDate",
                unique: true,
                filter: "[BranchId] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InventorySnapshots_BranchId_SnapshotDate",
                table: "InventorySnapshots");

            migrationBuilder.DropIndex(
                name: "IX_InventorySnapshots_SnapshotDate",
                table: "InventorySnapshots");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "InventorySnapshots");

            migrationBuilder.CreateIndex(
                name: "IX_InventorySnapshots_SnapshotDate",
                table: "InventorySnapshots",
                column: "SnapshotDate",
                unique: true);
        }
    }
}

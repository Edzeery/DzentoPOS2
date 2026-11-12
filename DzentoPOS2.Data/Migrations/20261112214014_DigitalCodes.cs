using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DzentoPOS2.Data.Migrations
{
    /// <inheritdoc />
    public partial class DigitalCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DigitalCode",
                table: "SalesInvoiceItems",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DigitalCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Pin = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    VariantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    SalesInvoiceId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SalesInvoiceItemId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DigitalCodes_ProductVariants_VariantId",
                        column: x => x.VariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DigitalCodes_SalesInvoiceId",
                table: "DigitalCodes",
                column: "SalesInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalCodes_Status",
                table: "DigitalCodes",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalCodes_VariantId_Code",
                table: "DigitalCodes",
                columns: new[] { "VariantId", "Code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DigitalCodes");

            migrationBuilder.DropColumn(
                name: "DigitalCode",
                table: "SalesInvoiceItems");
        }
    }
}

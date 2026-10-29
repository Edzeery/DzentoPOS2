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
            // Both statements are idempotent: a database created from a model
            // snapshot that already lacks IX_CashSessions_CashboxId (or whose
            // index was removed), or one that already carries the unique index
            // from an earlier run of this migration under a different id, must
            // never abort startup here.
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_CashSessions_CashboxId\";");

            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX IF NOT EXISTS \"UX_CashSessions_OpenPerCashbox\" "
                + "ON \"CashSessions\" (\"CashboxId\") "
                + "WHERE \"Status\" = 0 AND \"IsDeleted\" = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"UX_CashSessions_OpenPerCashbox\";");

            // Symmetrical with Up(): only recreate the plain index when missing.
            migrationBuilder.Sql(
                "CREATE INDEX IF NOT EXISTS \"IX_CashSessions_CashboxId\" ON \"CashSessions\" (\"CashboxId\");");
        }
    }
}

# .NET 10 startup smoke run (P2 / Step 5)

The real startup path — `DbInitializer.InitializeAsync` (schema repair →
`MigrateAsync` → seeders → store bootstrap), exactly what `Program.cs` calls —
was executed against **copies of the real databases** in the repository. Nothing
in `DzentoPOS2/Database/DzentoPOS.db`, `DzentoPOS2/dzento_design.db` or
`temp-legacy.db` was modified (all work happened on `%TEMP%` copies).

## Starting states

| Copy of | Migration history | Cash-session columns |
| --- | --- | --- |
| `DzentoPOS2/Database/DzentoPOS.db` | up to `AddProductBarcode` | **absent** |
| `temp-legacy.db` | includes the removed `20261023131151_E3` id | present |
| `DzentoPOS2/dzento_design.db` | through `20261023140849_E3` | present |

## Result

All three copies initialised successfully:

| Copy | Migrations applied | Last migration | Seed rows |
| --- | --- | --- | --- |
| dev (`DzentoPOS.db`) | 9 | `E3_UniqueOpenSessionIndex` | 1 user, 43 permissions, 6 currencies, 1 branch, 1 cashbox |
| legacy (`temp-legacy.db`) | 9 | `E3_UniqueOpenSessionIndex` | 1 user, 43 permissions, 6 currencies, 1 branch, 1 cashbox |
| design (`dzento_design.db`) | 9 | `E3_UniqueOpenSessionIndex` | 1 user, 43 permissions, 6 currencies, 1 branch, 1 cashbox |

After the run every copy exposes the full current schema (`CashTransactions`
with `CashSessionId`/`SourceId`/`SourceType`, `SalesInvoices` with
`CashSessionId` and the F2 discount columns) and the orphaned history row of the
removed E3 migration is gone.

## No data loss

Row-count comparison between each source and the upgraded copy: **no table lost
rows**. `DzentoPOS.db` went from 151 to 154 rows (all 151 preserved; +1 cashbox
and +2 app settings added by seeding). `temp-legacy.db` and `dzento_design.db`
are empty schema-only databases (0 rows before), so only schema and seed data
changed.

## Warnings observed (informational)

1. `Microsoft.EntityFrameworkCore.Model.Validation[10400]` — sensitive data
   logging enabled (Debug builds only, pre-existing).
2. `Microsoft.EntityFrameworkCore.Migrations[20410]` — the migration operation
   `PRAGMA foreign_keys = 0;` from `E3_CashSessionLinking` cannot run inside a
   transaction. This is inherent to that migration's SQLite table rebuild (FK
   change), not a regression.

Neither warning affects the outcome; both copies report `OK`.

# .NET 10 retarget verification (P2 / Step 4)

> **Historical (reverted).** This documents the .NET 10 retarget, which was reverted because Visual Studio 2022 cannot build `net10.0`. The solution now targets .NET 9 — see [`net9-retarget.md`](net9-retarget.md) for the current verification.

All commands run from the repository root with the pinned SDK
(`%USERPROFILE%\.dotnet\dotnet.exe`, 10.0.403; see `global.json`).

## Build

```
dotnet build DzentoPOS2.sln -t:Rebuild
```

| | Baseline (.NET 8) | After retarget (.NET 10) |
| --- | --- | --- |
| Errors | 0 | **0** |
| Warnings | 21 | **21** |

Every warning is `NU1701` (packages restored for .NET Framework targets:
`AForge*`, `OpenTK*`, `SkiaSharp.Views.WindowsForms`). These are pre-existing
and unchanged; no new warnings were introduced by the retarget.

## Tests

```
dotnet test DzentoPOS2.sln --no-build
```

| Suite | Result |
| --- | --- |
| DzentoPOS2.UnitTests | **84 / 84 passed** |
| DzentoPOS2.IntegrationTests | **41 / 41 passed** |

Both match the pre-retarget baseline.

## Migrations

- **Model vs snapshot:** `dotnet ef migrations has-pending-model-changes
  --project DzentoPOS2.Data` → *"No changes have been made to the model since the
  last migration."*
- **Fresh database:** `dotnet ef database update --connection "Data Source=<temp>"`
  on an empty file applies all 9 migrations. The resulting schema contains the
  cash-session columns that the E3 table rebuild depends on
  (`CashTransactions.CashSessionId/SourceId/SourceType`,
  `SalesInvoices.CashSessionId`).
- **Legacy upgrades:** the 9 legacy-repair/upgrade integration tests pass
  (`LegacySchemaRepairTests`, `SaleServiceE3Tests.Legacy_*`), covering a database
  whose history still references the removed `20261023131151_E3` id as well as the
  pre-E3 states.

The EF warning *"migration 'E3_CashSessionLinking' ... cannot be executed in a
transaction"* is informational (SQLite rebuilds the table for `AddForeignKey`)
and is inherent to that migration, not a regression.

## Model snapshot regeneration

EF Core 10 rewrites `AppDbContextModelSnapshot.cs` even when no model change is
detected. Verified with a throwaway probe migration:

```
dotnet ef migrations add P2SnapshotProbe --project DzentoPOS2.Data
```

- **Up/Down are empty** — proof that the generated migration contains **no schema
  change**.
- The only snapshot difference is the version annotation:
  `ProductVersion` `"8.0.27"` → `"10.0.14"` (1 line).

The probe migration was deleted and **no new migration was added**. The snapshot
annotation alone was regenerated (1-line change, exactly EF 10's own output);
`has-pending-model-changes` is still clean afterwards and the existing migrations
were **not** edited or renamed.

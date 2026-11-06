# Migration evidence — real database copy (2026-11-06)

**Item 6.** Proves that the production startup migration path is safe on a copy of
the live database and records the measured usage of the four legacy inventory
columns targeted by the removal plan (ADR-11).

The live database is **never opened for writing**: the audit only copies it. All
migration work runs on the copy and the copy is deleted afterwards.

## Tool

```
DzentoPOS2.WinForms.exe --migration-audit [--source <db>] [--work <dir>] [--keep]
```

Implemented in `DzentoPOS2/Diagnostics/MigrationAudit.cs`, dispatched from
`Program.cs` before any application service is built.

| Input | Value |
|---|---|
| Source | `%LocalAppData%\DzentoPOS2\Database\DzentoPOS.db` |
| Size | 1,118,208 bytes |
| SHA-256 before audit | `4F6207EEB1D5D04BE7A9C60AD814840E806162A0686B551E4DAD5FE1DCD82A4D` |
| `-wal` / `-shm` present | no |
| Machine clock | 2026-11-06 |

## Method

1. Copy the database and any `-wal` / `-shm` companions into a temp working folder.
2. `PRAGMA wal_checkpoint(TRUNCATE)` on the copy so the WAL is merged into the copy.
3. Snapshot **before**: `PRAGMA integrity_check`, `PRAGMA foreign_key_check`,
   schema (`sqlite_master` + `PRAGMA table_info`), one SHA-256 per row keyed by its
   primary key, and `__EFMigrationsHistory`.
4. Run the **real** `DbInitializer.InitializeAsync` against the copy: WAL mode →
   `LegacySchemaRepair` → `EF MigrateAsync` → every seeder → `StoreBootstrapper`.
5. Snapshot **after**, re-hashing rows with the *before* column set so a newly
   added column can never look like a changed row.
6. Compare tables/columns lost, rows removed/changed/added and migrations removed,
   then query legacy-column usage.

## Result — PASS

| Check | Before | After | Verdict |
|---|---|---|---|
| `integrity_check` | `ok` | `ok` | no corruption |
| `foreign_key_check` rows | 0 | 0 | no FK violation |
| Tables | 60 | 60 | none lost |
| Columns | all present | all present | none lost |
| Rows removed | — | **0** | no data loss |
| Rows changed | — | **0** | no in-place rewrite |
| Rows added | — | **2** | seeded `Customers` (walk-in accounts) |
| `__EFMigrationsHistory` | 15 | 15 | 0 removed |
| Journal mode | `wal` | `wal` | unchanged |

The two added rows are the seeded customer accounts
(`13681685-A684-4A63-8BA4-A46AFA226A65`, `BEE79859-3815-42E7-AB3D-A21AFCFA737C`);
additions are expected from `CustomerAccountSeeder` and are not user data.

Machine-readable evidence: `docs/evidence/migration-audit.json`.

## Legacy column usage measured on the real data

| Column / metric | Rows |
|---|---|
| `Products.Total` | 2 |
| `Products.Barcode` non-empty | 2 |
| `Products.CostPriceAmount` non-zero | 2 |
| `Products.CostPriceCurrency` set | 2 |
| `ProductVariants.Total` | 2 |
| `ProductVariants.Name` non-empty | 2 |
| `ProductVariants.AdditionalPrice` non-zero | 0 |
| `ProductBarcodes.Total` | 2 |
| Products whose default-variant cost differs from `Product.CostPrice` | 0 |

Interpretation:

- **`Product.Barcode`** is still populated (2/2) but already backfilled into
  `ProductBarcodes` (2 rows) by migration `20261105165317_CatalogPhase1_DataModel`,
  so the legacy column is redundant.
- **`Product.CostPrice`** is populated and currently equals every default
  variant's cost (0 divergences), which makes it a duplicate of the variant cost.
- **`ProductVariant.Name`** is populated (2/2) and only used for display.
- **`ProductVariant.AdditionalPrice`** is **unused** (0 non-zero) — no writer and,
  outside the entity/configuration, no reader.

## Removal plan (detail for ADR-11)

No column is dropped now. The plan is three gated phases; each phase ships a
release and is verified by `--migration-audit` plus the two test suites.

### Phase A — stop writing (one release)

| Legacy member | Written today | Phase A write target |
|---|---|---|
| `Product.Barcode` | `ProductService.CreateAsync/UpdateAsync` (`102`, `185`) | `ProductBarcode` rows only |
| `Product.CostPrice` | `ProductService` (`108`, `191`), `PurchaseService` (`219`) | default `ProductVariant.CostPrice` only |
| `ProductVariant.Name` | `ProductVariantsDialog` generation | variant attribute values |
| `ProductVariant.AdditionalPrice` | nothing | (already unused) |

Columns/rows are left intact so a rollback to the previous release is lossless.
Existing reports keep reading the legacy values for this one release.

### Phase B — stop reading (one release)

- Barcode lookup (`ProductService` projection `340`) and POS scanning
  (`SalesForm` `901`) read `ProductBarcodes` only.
- Cost/profit reads (`SaleService` `266`/`285`, `StockService` `368`,
  `DashboardService` `191`/`280`) resolve through the default variant.
- `ProductVariantsDialog` (`58`, `282`) renders attribute values with
  `Product.Name` as fallback instead of `ProductVariant.Name`.
- A guard test fails the build if production code references any of the four
  members again.

### Phase C — drop (one migration)

- Precondition: `--migration-audit` reports 0 non-empty `Product.Barcode`, zero
  production references and a clean run.
- One migration drops `Products.Barcode`, `Products.CostPriceAmount`,
  `Products.CostPriceCurrency`, `ProductVariants.Name`,
  `ProductVariants.AdditionalPrice` (and `IX_Products_Barcode`).
- `ProductId` and **every** `ProductVariants` row are preserved; no row is
  deleted. `Down()` re-adds the columns as nullable.

## Reproduce

```powershell
dotnet build DzentoPOS2.sln
dotnet run --project DzentoPOS2 --no-build -- --migration-audit
```

Exit code `0` means the audit passed (no table/column/row loss, clean integrity,
no FK regression, no migration removed). Add `--keep` to inspect the migrated
copy, or `--source <db>` to audit another database.

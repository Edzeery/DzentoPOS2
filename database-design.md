# Database Design — DzentoPOS2

The application persists to **SQLite** through EF Core 8 (`AppDbContext` in
`DzentoPOS2.Data`). The physical schema is created and migrated automatically at
startup (`context.Database.MigrateAsync()`), with `LegacySchemaRepair` fixing
older/partially-created databases before the migrations run.

All entities derive from `AuditableEntity`, so most tables carry `Id`
(`Guid`), `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`, `IsDeleted` and
`IsActive`. Monetary columns are `decimal(18,2)`; percent columns `decimal(5,2)`.
Enumerations are stored as `INTEGER`.

## Sales → Stock → Cash flow (tables and links)

```
Customers ─┐
           ├─< SalesInvoices ──< SalesInvoiceItems ──> Products
CashSessions ─┘     │  │                                  │
   │                │  └──< SalesPayments                │
   │                │                                     ▼
   └─< CashTransactions            ProductStocks ─< InventoryTransactions
        (SourceType/SourceId)               ▲              (SaleOut / VoidIn)
                                            └── StockMovements
```

| Table | Purpose | Key columns / links |
|---|---|---|
| `SalesInvoices` | One committed sale (or credit sale). | `CashSessionId` → `CashSessions` (`Restrict`), `CustomerId` → `Customers`, `IsVoided`, `Subtotal`, `DiscountAmount`, `TaxAmount`, `Total`, `FeesAmount`, `PaidAmount`, `RemainingAmount`, `InvoiceNumber` (from `DocumentSequences`). |
| `SalesInvoiceItems` | Lines of a sale. | `SalesInvoiceId` → `SalesInvoices`, `ProductId` → `Products`, `Quantity`, `Price`, `Total`, `UnitCost` (frozen moving avg), `ListPrice` (frozen product sale price), `DiscountAmount` (line share), `IsCostEstimated`, `Description` (≤ 1000). |
| `SalesPayments` | Payments taken for a sale. | `SalesInvoiceId` → `SalesInvoices`, `PaymentMethod` (`Cash`, `Card`, …), `Amount`. Only **cash** rows affect the drawer. |
| `CashSessions` | One open drawer per cashbox per business day. | `CashboxId`, `Status` (`CashSessionStatus`), `OpeningBalance`, `ClosingBalance`, `ExpectedBalance`, `Difference`, `OpenedAtUtc`. Filtered **unique** index `UX_CashSessions_OpenPerCashbox` on `CashboxId` where `"Status" = 0 AND "IsDeleted" = 0`. |
| `CashTransactions` | Ledger of every drawer movement. | `CashboxId`, `CashSessionId` → `CashSessions` (`Restrict`), `Amount`, `TransactionType` (`MovementType`), `SourceType` + `SourceId` (polymorphic link back to the sale / expense / movement). |
| `Cashboxes` | Drawers belonging to a branch. | `CurrentBalance`. |
| `StockMovements` / `InventoryTransactions` | Stock ledger. | `ProductId`, `Quantity`, direction; sale writes `SaleOut`, void writes `VoidIn`. |
| `ProductStocks` | Current quantity per product / warehouse. | `ProductId`, `WarehouseId`, `Quantity`. |
| `AppSettings` | Key/value settings. | `Key` (unique), `Value`, `Group`. `sales.max_discount_percent` seeds here (see below). |
| `DocumentSequences` | Daily document counters. | `Prefix`, `Date`, `LastNumber`. |

## Referential integrity (E3)

- `CashTransactions.CashSessionId` → `CashSessions.Id` — `ON DELETE RESTRICT`
  (migration `20261023140849_E3_CashSessionLinking`).
- `SalesInvoices.CashSessionId` → `CashSessions.Id` — `ON DELETE RESTRICT`.
- `CashSessions` unique filtered index (migration
  `20261027164243_E3_UniqueOpenSessionIndex`): at most one *open*, non-deleted
  session per cashbox. This is the database-level guard behind
  `ICashSessionService.OpenSessionAsync`.

## Money and time

- Money is rounded once per line and once for the totals with
  `MidpointRounding.AwayFromZero` at the currency's decimal-digit precision
  (`InvoiceCalculator`). Stored totals are exactly the calculator output.
- Timestamps are stored as **UTC** (`*AtUtc`, `CreatedAt`). The local business
  day is derived through `IBusinessClock` (`RegionalContext.ToLocal`); the
  cash-drawer policy uses the local day, not the UTC day.
- SQLite cannot aggregate `decimal` server-side ("cannot apply aggregate
  operator 'Sum' on expressions of type 'decimal'"), so session totals are
  summed **in memory** from the loaded rows (`CashSessionService`).

## Seeding and migrations

- `DbInitializer` runs the migrations and then the idempotent seeders
  (`AppSettingSeeder`, `CashboxSeeder`, branch/warehouse bootstrap).
- `AppSettingSeeder` only inserts keys that are missing, so re-running it never
  overwrites a value a user changed — including `sales.max_discount_percent`.
- New migrations are generated with `dotnet ef migrations add <Name>` against
  `DzentoPOS2.Data` (see `AppDbContextFactory`); the model snapshot is the
  source of truth for the "one open session per cashbox" index.

## Catalog — Products, Variants, Barcodes, Images, Activity Modules (planned)

> Status: **design only (Phase 0)**. Implementation follows in Phases 1–7.

### Entities (target)

- **Product (parent)** — `Name`, `Brand`, `CategoryId`, `UnitId`, `Description`, `ProductKind`,
  `TrackStock` (default), `IsActive`. Holds defaults inherited by its default variant.
  - New: `ProductKind` enum: `Standard`, `Weighed`, `Serialized`, `Service`, `DigitalCode`.
  - New: `Brand` (string, nullable).
  - New: `ImagePath` / `ThumbnailPath` (relative paths, primary image).
- **ProductVariant** — real variant: `ProductId` (FK cascade), `SKU` (nullable, unique),
  `CostPrice` (Money), `SalePrice` (Money), `MinimumStock` (Quantity), `TrackStock`,
  `IsActive`, `IsDefault` (hidden default variant per product), `ImagePath`/`ThumbnailPath`.
  - Replaces the current `Name + AdditionalPrice` only model.
- **ProductBarcode** — `VariantId` (FK cascade) + `Barcode` (required, max 64) +
  `IsPrimary`. Global unique index on `Barcode` by default; duplicates allowed only when
  `catalog.allow_duplicate_barcodes` setting is true (POS scan then shows a picker).
- **AttributeDefinition / AttributeValue / VariantAttribute** — `AttributeDefinition(ProductId, Name, SortOrder)`,
  `AttributeValue(DefinitionId, Value)`, `VariantAttribute(VariantId, DefinitionId, ValueId)`.
- **ProductImage** (gallery, optional Phase 3) — `OwnerType` (Product/Variant), `OwnerId`,
  `RelativePath`, `ThumbnailPath`, `SortOrder`, `IsPrimary`.

### Stock & transactions (target)

- `ProductStock` gains `VariantId` (FK, required) — stock per variant/warehouse.
- `InventoryTransaction` gains `VariantId` (FK, required) — ledger per variant.
- `SalesInvoiceItem` gains `VariantId` (FK, required) — keep `ProductId` for compatibility/reports.
- `PurchaseInvoiceItem` gains `VariantId` (FK, required) — keep `ProductId`.
- All sale/purchase/stock/profit logic operates on `VariantId`. `ProductId` retained for
  reporting and backward compatibility.

### Simple products

- Every product receives a hidden default variant (`IsDefault = true`) on creation and via
  migration backfill. Product-level price/cost become the default variant's price/cost.

### Barcodes

- `Product.Barcode` (legacy column) is kept read-only for compatibility; new code uses
  `ProductBarcode` rows linked to a variant. Migration backfills existing `Product.Barcode`
  into the default variant's barcodes (IsPrimary = true).
- Unique index on `Product.Barcode` is dropped in favour of `ProductBarcode.Barcode`.

### Images

- Files on disk under the app data folder, relative paths in DB. Resize to 1024px (longest
  side) JPEG + 256px thumbnail. System.Drawing only. Lazy load + LRU + strict Dispose.

### Activity modules (feature flags)

- Stored as `AppSettings` rows (group `Modules`): `modules.grocery.enabled`,
  `modules.phones.enabled`, `modules.digital.enabled`, `modules.general.enabled`.
- Selected in the first-run wizard and editable later in Settings; disabling a module never
  deletes data.

### Migration strategy

- Non-destructive, safe on SQLite (table rebuild where needed). Backup before migration.
- Backfill `VariantId` in `SalesInvoiceItem`, `PurchaseInvoiceItem`, `ProductStock`,
  `InventoryTransaction` to the product's default variant.
- Keep `ProductId` columns for compatibility and reporting.
- Unique-index changes done with `DROP INDEX IF EXISTS` / `CREATE ... IF NOT EXISTS` so
  re-running is safe (see F3b precedent).

### Stock deduction path (current, for reference)

- `SaleService.CreateInternalAsync` loads `Products` by `ProductId`, freezes
  `UnitCost = CostPrice`, `ListPrice = SalePrice` per line, then calls
  `IStockService.ApplySaleAsync(invoiceId, ..., warehouseId, StockChange(ProductId, Qty), ...)`.
- `VoidAsync` calls `IStockService.ReverseAsync(..., StockChange(ProductId, Qty), MovementType.SaleVoidIn, ...)`.
- Cost is moving weighted average (PurchaseService). Profit uses frozen `UnitCost`/`ListPrice`.
- Phase 4 moves these to `VariantId` while keeping profit numbers identical for simple products.


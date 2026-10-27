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
| `SalesInvoices` | One committed sale (or credit sale). | `CashSessionId` → `CashSessions` (`Restrict`), `CustomerId` → `Customers`, `IsVoided`, `Total`, `PaidAmount`, `RemainingAmount`, `InvoiceNumber` (from `DocumentSequences`). |
| `SalesInvoiceItems` | Lines of a sale. | `SalesInvoiceId` → `SalesInvoices`, `ProductId` → `Products`, `Description` (≤ 1000). |
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

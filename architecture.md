# Architecture — DzentoPOS2

DzentoPOS2 is a layered .NET 9 WinForms point-of-sale application over SQLite.

## Projects

| Project | Responsibility |
|---|---|
| `DzentoPOS2.Core` | Domain entities, value objects, enums, `InvoiceCalculator`, business exceptions (`DomainException`, `CodedDomainException`, `CashSessionRequiredException`). No EF / UI references. |
| `DzentoPOS2.Shared` | Cross-layer DTOs (`Contracts/DTOs/...`), `RegionalContext`, `AppSettingKeys`-adjacent helpers. |
| `DzentoPOS2.Data` | `AppDbContext`, entity configurations, repositories, migrations, seeders (`AppSettingSeeder`, `CashboxSeeder`), `LegacySchemaRepair`. |
| `DzentoPOS2.App` | Application services (`ISaleService`, `ICashSessionService`, `IExpenseService`, …), `ISalesEventPublisher`, diagnostics (`ExceptionFormatter`, `ExceptionLogger`), session/authorization. |
| `DzentoPOS2.Localization` | `AppStrings` resources (base + `en` / `ar` / `fr`) with matching `AppStrings.Designer.cs`. |
| `DzentoPOS2` (WinForms) | Forms, custom controls, themes, navigation, DI composition (`Infrastructure/DependencyInjection.cs`). |
| `tests/*` | xUnit unit tests and integration tests (real migrations on a temp SQLite file). |

## Layers and dependency direction

```
WinForms  ──►  App  ──►  Data  ──►  Core
    │            │          │         ▲
    └────────────┴──────────┴─────────┘
              Shared (DTOs / helpers)
```

Forms depend on service **interfaces**; services depend on repositories / the
`AppDbContext`; only `Data` knows about EF Core and SQLite.

## Services

- `ISaleService` — creates / voids sales. Validates the cart, calls
  `InvoiceCalculator` for the totals, enforces the discount ceiling, requires an
  open cash session, decrements tracked stock and writes the cash ledger.
- `ICashSessionService` — opens / closes drawers, records pay-in / pay-out and
  produces the session summary (`GetSessionSummaryAsync`).
- `IStockService` — the single entry point for stock changes (`SaleOut`,
  `VoidIn`, adjustments) and the negative-stock guard.
- `IExpenseService` — records an expense and, when cash, the linked
  `Purchase` cash row (`SourceType = "Expense"`).
- `ISalesEventPublisher` — singleton in-process event bus (see below).

## The sale → stock → cash → event flow

A checkout (`SaleService.CreateAsync`) runs inside one database transaction:

1. **Validate.** `InvoiceCalculator.Calculate(...)` produces `InvoiceTotals`;
   domain rules throw `CodedDomainException` with a stable
   `DomainErrorCodes` code. The discount ceiling is checked against the
   effective percentage (`DiscountAmount / Subtotal * 100`) for **both** discount
   modes.
2. **Cash session.** The service loads the open session for the store's cashbox.
   Without one it throws `CashSessionRequiredException` and **nothing is
   persisted**.
3. **Stock.** For each tracked line `IStockService` writes a `SaleOut`
   movement and updates `ProductStocks`; insufficient stock throws
   `CodedDomainException(InsufficientStock)`.
4. **Persist.** `SalesInvoice` + `SalesInvoiceItems` (+ `SalesPayments`) are
   saved, then a `CashTransaction` (`SaleCashIn`) is linked to the session only
   for the cash that was actually taken. Card sales never touch the drawer.
5. **Commit.** `SaveChangesAsync()` + `CommitTransactionAsync()`.
6. **Publish (after commit).** Only once the transaction is committed does the
   service raise `ISalesEventPublisher.SaleCompleted`. A subscriber that reads
   the database therefore always observes the committed invoice, payment and
   cash row.

`VoidAsync` mirrors the flow: it reverses stock (`VoidIn`) and, only for the
cash that was really taken (looked up from `SalesPayments`), writes a
`SaleVoidCashOut` row, then raises `SaleVoided` after the commit. A voided card
sale never moves the drawer.

```
SalesForm ─► ISaleService ─► InvoiceCalculator (totals + rules)
     │            │
     │            ├─► IStockService  ─► StockMovements / ProductStocks
     │            ├─► AppDbContext   ─► SalesInvoices / SalesPayments / CashTransactions
     │            └─► ISalesEventPublisher ──(after commit)──┐
     │                                                       ▼
     └─ (subscribes) ─────────────► MainForm status-bar chip / CashSessionForm KPIs
```

## Live cash-drawer updates

`ISalesEventPublisher` is registered as a **singleton** so every scoped service
and every open form shares the same notifications. Each subscriber owns its
lifetime and unsubscribes in `Dispose()`; the publisher never touches a control
(WinForms subscribers marshal to the UI thread themselves). A failing handler is
isolated so it cannot break the sale.

- `MainForm` subscribes to `SaleCompleted` / `SaleVoided` / `CashMovementAdded`
  and repaints the status-bar drawer chip; it also warns when the open session
  was opened on a previous local business day
  (`CashSessionPolicy.IsFromPreviousBusinessDay`).
- `CashSessionForm` coalesces the same events into a single refresh of its KPI
  cards (`GetSessionSummaryAsync` is the single source of truth).

## Cross-cutting concerns

- **Localization.** Every user-facing string comes from `AppStrings`; the four
  resx files and `AppStrings.Designer.cs` are kept at key parity. Domain errors
  travel as stable codes and are localized by `ExceptionFormatter.LocalizeCode`,
  so the UI never matches on an English message.
- **Theming / DPI.** `AppColors` resolves semantic colours from `ThemeManager`
  (light/dark). Forms are designer-first (layout in `<Form>.Designer.cs`, logic
  in `<Form>.cs`) and apply the theme and localized text at runtime. The app is
  `PerMonitorV2` DPI-aware with `AutoScaleMode = Font`.
- **Time.** `IBusinessClock` exposes UTC plus the local business day derived from
  `RegionalContext`; the drawer policy is defined on the local day.
- **Errors.** `ExceptionFormatter` renders user-facing text (localized) and
  `ExceptionLogger` writes the full detail to a log file.

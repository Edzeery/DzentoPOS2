# Changelog

All notable changes to the DzentoPOS2 project are documented in this file.

## [Unreleased] - 2026-11-05 (Catalog system Phase 3 complete + dark mode)

### Added
- **Automatic variant combination generator.** New `ProductVariantsDialog` (tabbed: variants + attributes) lets a product define attributes and comma-separated values, then generates every value combination into a variant (name, prices seeded from the product) and links `VariantAttribute` rows. Generation is idempotent: existing combinations are detected by their value-id signature and skipped. New `VariantCombinationGenerator` (`DzentoPOS2.Core/Services/Inventory`) computes the cartesian product with no UI dependency and is covered by 5 unit tests.

### Changed
- **Consistent dark mode across dialogs.** New `ThemedDialog` base (`DzentoPOS2/Helpers/ThemedDialog.cs`) applies the central design tokens on load and repaints on `ThemeManager.ThemeChanged`. Adopted by the catalog dialogs (`ProductEditDialog`, `ProductVariantsDialog`, `ProductBarcodesDialog`), the first-run module wizard (`FirstRunModulesDialog`) and the data-entry dialogs (`SupplierEditDialog`, `CustomerEditDialog`, `ExpenseEditDialog`, `RoleEditDialog`, `UserEditDialog`, `PurchaseEditDialog`, `ProductCategoryEditDialog`, `ProductUnitEditDialog`, `StockAdjustDialog`, `SalesCalculatorDialog`, `ChangePasswordDialog`, `ForgotPasswordDialog`, `RecoveryKeyDisplayDialog`). The first-run wizard's confirm button now uses `AppColors.OnDark` so its label stays legible in both modes.
- `ProductVariantsDialog` guards variant deletion when a variant still holds non-zero stock.

### Verified
- Build **0 errors / 46 warnings** (`NU1701`), Unit **129/129**, Integration **70/70**.

## [Unreleased] - 2026-11-05 (Catalog system Phase 4)

### Changed
- **Stock, sales and purchases now operate on `VariantId`.** `StockChange` gained a `VariantId` (`DzentoPOS2.App/Services/Inventory/IStockService.cs`); `StockService` aggregates lines by `(ProductId, VariantId)`, tracks stock per `(ProductId, VariantId, WarehouseId)` and writes `VariantId` on every `InventoryTransaction`. Missing/default variants are resolved (and created) on the fly, so legacy callers that only know a `ProductId` keep working.
- `SaleService` writes `VariantId` on `SalesInvoiceItem` and passes the resolved variant to `ApplySaleAsync`; void reverses by the stored line `VariantId`.
- `PurchaseService` passes the resolved default variant to `ApplyPurchaseAsync` and reverses by the stored line `VariantId`.
- **Moving weighted average is now per variant.** The purchase cost update resolves the default variant, averages against that variant's on-hand quantity and updates its `CostPrice`; the legacy `Product.CostPrice` is kept in sync when the variant is the default, so simple-product costs and reports are unchanged.
- `InventoryProfitCalculator` aggregates stock by variant (`InventoryProductInput.VariantId`, `InventoryStockInput.VariantId`); simple-product results are identical (unit tests unchanged in expectation).
- `ProductService.GetProductStocksAsync` aggregates stock per warehouse so multi-variant products still show one row per warehouse.

### Verified
- Build **0 errors / 47 warnings** (pre-existing `NU1701`), Unit **124/124**, Integration **70/70**.

## [Unreleased] - 2026-11-05 (Catalog system Phase 0)

### Added
- Merged `fix/barcode-scanner` into `feature/dashboard-filters`; base branch now `feature/dashboard-filters`.
- Documentation: `database-design.md` and `architecture.md` extended with catalog (variants, barcodes, images, activity modules, migration strategy, stock deduction path).
- `docs/decisions.md` with 10 ADRs for the catalog system.
- `Todos.md` section K tracking phases K.0â€“K.7.

## [Unreleased] ï؟½?? 2026-11-02 (Profit rebuild phases 1-4)
- Phase 1 (data): `SalesInvoiceItem` gained `UnitCost`, `ListPrice`, `DiscountAmount`, `IsCostEstimated`; `SalesInvoice` gained `FeesAmount`. Migration `ProfitRebuild_Phase1` backfills legacy rows (UnitCost from current Product.CostPrice, ListPrice from line Price, IsCostEstimated=true).
- Phase 2 (invoice): `SalesInvoice.Create` keeps discount-before-tax; DTOs gained `Fees`. Currency unified via RegionalContext.
- Phase 3 (stock/cost): `PurchaseService.CreateAsync` applies moving weighted average to `Product.CostPrice` for tracked products; stock already handled by StockService.
- Phase 4 (frozen cost): `SaleService.CreateAsync` freezes `UnitCost`/`ListPrice`/line discount share and `FeesAmount`; added `ProfitCalculator` (Core) with comprehensive unit tests; added `ProfitFlowTests` integration test.

## [Unreleased] â€” 2026-10-29 (G1.3 security, authorization, audit)

### Added

#### G1.3 â€” Security hardening (E7)

- **Forced password change** â€” `User.MustChangePassword` (`DzentoPOS2.Core/Entities/Identity/User.cs:26`, default `false` in `DzentoPOS2.Data/Configurations/Identity/UserConfiguration.cs:30`, migration `20261029145702_G1_3_MustChangePassword`). `AdminSeeder` flags the seeded account and re-flags any admin that still verifies against the well-known default password (`DzentoPOS2.Data/Seed/AdminSeeder.cs:24-47`). `LoginResponseDto`/`UserSession` carry the flag, `LoginForm` opens the new blocking `ChangePasswordDialog` (`DzentoPOS2/Forms/Auth/ChangePasswordDialog.cs:16`) before raising `LoginSuccess`, and cancelling signs the user back out (`DzentoPOS2/Forms/Auth/LoginForm.cs:502-545`). `AuthService.ChangePasswordAsync` clears the flag.
- **Service-layer authorization** â€” new `IAuthorizationGuard` (`DzentoPOS2.App/Authorization/AuthorizationGuard.cs:25`). `DemandAsync` throws a coded domain exception (`DomainErrorCodes.PermissionDenied = "auth.permission_denied"`, `DzentoPOS2.Core/Exceptions/CodedDomainException.cs:27`) that `ExceptionFormatter` maps to `AppStrings.AccessDenied` (`DzentoPOS2.App/Diagnostics/ExceptionFormatter.cs:141`), so the UI shows a translated message instead of an English exception. SuperAdmin/`IsAdmin` keep the full-access bypass. Enforced in `UserService` (create/update/delete, L58/118/178), `RoleService` (L54/101/175), `ProductService` (L91/147/180) and `SaleService` (create L142, void L355).
- **Audit log** â€” new `IAuditService` (`DzentoPOS2.App/Authorization/AuditService.cs:28`) writes through its own short-lived `AppDbContext` so an audit row never commits the caller's pending work and a failed write only logs a warning. Records `Login`, `Logout`, `ChangePassword`, one `SuperAdminBypass` per privileged sign-in and every `PermissionDenied` (`NewValues` = permission code).
- **Permission catalog** â€” added `CashSessionsOpen`, `CashSessionsClose`, `UnitsCreate`, `UnitsEdit`, `UnitsDelete` (`DzentoPOS2.Core/Constants/Permissions.cs`) and granted them in `RolePermissionSeeder`.

### Fixed

- `AuthService.ChangePasswordAsync` now reads the tracked user (`GetWithRolesAsync`) instead of the no-tracking `GetByIdAsync`; with the G1.2 scope-per-page change the login call already tracks that user in the same page scope, so attaching a detached copy threw "another instance with the same key is already being tracked".
- `SaleServiceE3Tests` fixtures now sign in an admin session explicitly, because sales/product creation is a guarded operation.

### Verified

- `SecurityG13Tests` (new, `tests/DzentoPOS2.IntegrationTests/SecurityG13Tests.cs`) â€” 7 integration tests:
  - forced change flagged at login and cleared after the change, including re-login,
  - `Login` and `ChangePassword` rows in `AuditLogs`,
  - guard denies a user without the permission, writes a `PermissionDenied` row, and returns `auth.permission_denied`,
  - guard allows an admin without a permission row,
  - guard denies an unauthenticated caller,
  - seeded admin flagged, re-seed idempotent (no duplicate account),
  - re-seed does not force a change after the password was replaced.
- Build **0 errors / 21 warnings** (all `NU1701`), Unit **84/84**, Integration **51/51**, `dotnet ef migrations has-pending-model-changes` clean, application starts with scope validation enabled and no logged exceptions (25 s run).

## [Unreleased] â€” 2026-10-29 (G1.4 data paths, PRAGMA interceptor, online backup)

### Added

#### G1.4 â€” Data paths, PRAGMA interceptor, online backup (E4/E5)

- **Per-user data paths** â€” new `AppPaths` (`DzentoPOS2.Core/Configuration/AppPaths.cs`) relocates the SQLite DB, `user-settings.json`, theme file, store branding and logs to `%LocalAppData%\DzentoPOS2\`. One-time migration from the legacy BaseDirectory copies + verifies (length + contents) without overwriting or moving the original; the `-wal` sidecar is included. `UserPreferencesService` reads from `AppPaths.UserSettingsPath`, `ThemeManager` stores `theme.json`, `StoreBranding` reads/writes `branding.json`, `ExceptionLogger` writes to `logs\` â€” all under `%LocalAppData%\DzentoPOS2\`.
- **PRAGMA interceptor** â€” new `SqliteConnectionInterceptor` (`DzentoPOS2.Data/Interceptors/SqliteConnectionInterceptor.cs`) sets `foreign_keys=ON`, `busy_timeout=5000`, `synchronous=NORMAL` on `ConnectionOpened`; the `SaveChangesAsync` PRAGMA override was removed from `AppDbContext`. `SqliteStartup` (`DzentoPOS2.Data/SqliteStartup.cs`) sets `journal_mode=WAL` once at startup via `EnsureWalModeAsync` outside transactions, called from `DbInitializer` and `Program`.
- **Online backup** â€” new `BackupService` (`DzentoPOS2.App/Services/Systems/BackupService.cs`) uses the SQLite online backup API (`SqliteConnection.BackupDatabase`, not raw file copy) with retention of the last 7 backups, automatic backup on close (max once per day), backup-file validation before restore, and restore with a safety backup + validation + application restart. `SettingsForm` gained a Backup/Restore card (`CreateBackupAsync`, `RestoreAsync`, `ApplyRetention`, `ValidateBackupAsync`).
- **Localization keys** â€” added `Backup`, `BackupFolder`, `BackupNow`, `BackupFileInvalid`, `BackupHint`, `BackupCreated`, `RestoreBackup`, `RestoreConfirm` to base/`ar`/`en`/`fr` + `AppStrings.Designer.cs` (parity diff 0).

### Verified

- `LocalizationParityTests.Every_culture_has_the_same_keys_as_the_base` â€” base vs ar/en/fr diff = 0.
- Build **0 errors / 21 warnings** (all `NU1701`), Unit **84/84**, Integration **51/51**, `dotnet ef migrations has-pending-model-changes` clean.

## [Unreleased] â€” 2026-10-29 (Admin emergency password reset)

### Added

#### Admin emergency password reset
- **Emergency reset via env var / config** â€” `AdminSeeder` now supports an opt-in reset triggered by setting `ADMIN_EMERGENCY_RESET=true` (environment variable) or `Admin:EmergencyReset=true` (appsettings.json / command line). When enabled, the built-in admin password is reset to the default (`admin`) and `MustChangePassword` is set, so the next sign-in forces a password change before the shell opens. The reset is logged via `ILogger.LogWarning`. Normal startup (flag absent) never overwrites the password, so the mechanism is safe to leave wired in.

### Usage

**Method A â€” Environment variable (recommended):**
1. Close the application.
2. Open Command Prompt and run: `set ADMIN_EMERGENCY_RESET=true`
3. Start the application from the same Command Prompt window.
4. Log in with `admin` / `admin` â€” the forced password-change dialog appears.
5. Set a new password and close the Command Prompt.

**Method B â€” appsettings.json:**
1. Close the application.
2. Open `appsettings.json` in the application folder (e.g. `bin\Debug\net9.0-windows\`).
3. Add `"Admin": { "EmergencyReset": true }` to the JSON.
4. Start the application â†’ log in with `admin` / `admin` â†’ change password.
5. Remove the `"EmergencyReset": true` line from `appsettings.json`.

**Method C â€” Sentinel file (simplest, no editing):**
1. Close the application.
2. Create an empty text file named `reset-admin-password.txt` in the application folder (next to the `.exe`).
3. Start the application â€” the file is consumed automatically and the password is reset.
4. Log in with `admin` / `admin` â†’ change password.

### Suggested future recovery methods (not yet implemented)
- **Email-based password reset**: Send a time-limited token to the registered email, verify it, then allow password change.
- **Security questions**: Set during account creation, used for self-service reset.
- **Recovery code**: A one-time code generated at first setup, stored securely offline.
- **Super-admin reset**: An already-logged-in super admin can reset another user's password via the Users page (already possible once logged in).

## [Unreleased] â€” 2026-10-29 (G1.2 scope per page)

### Fixed

#### G1.2 â€” No single `DbContext` for the whole session (E5)

- **DI scope validation** (`DzentoPOS2/Program.cs:34-39`): the DEBUG service provider is built with `ValidateScopes = true` and `ValidateOnBuild = true`, so any singleton capturing a scoped dependency now fails fast at startup instead of silently sharing state.
- **Data lifetimes** (`DzentoPOS2.Data/DependencyInjection.cs:43-71`): `AddDbContext<AppDbContext>` and `AddDbContextFactory<AppDbContext>` are both registered as `ServiceLifetime.Scoped`. Previously the singleton `IDbContextFactory<AppDbContext>` consumed the scoped `DbContextOptions<AppDbContext>`, which is exactly how one context leaked across the whole session.
- **`MainForm`** (`DzentoPOS2/Forms/Layout/MainForm.cs:50-53`, `84-103`): removed the unused, root-captured `AppDbContext` field/parameter. The shell only uses `IDbContextFactory<AppDbContext>.CreateDbContext()` for short-lived reads (`MainForm.cs:997`).
- **`NavigationService`** (`DzentoPOS2/Navigation/NavigationService.cs:9-30`, `75-115`): injects `IServiceScopeFactory` and creates one DI scope per page. The new page is resolved from its own scope; the previous page's scope is disposed right after the previous page is disposed, so its `DbContext`/unit of work never outlives the page.
- **`AppApplicationContext`** (`DzentoPOS2/Infrastructure/AppApplicationContext.cs`): injects `IServiceScopeFactory` and owns one scope per top-level window (login, shell). The login scope is disposed only after the login event has unwound (queued via `BeginInvoke`), and the shell scope is disposed when the main window closes.

### Verified

- `ScopeLifetimeTests` (new, `tests/DzentoPOS2.IntegrationTests/ScopeLifetimeTests.cs`) â€” 3 integration tests:
  - `ServiceProvider_BuildsWithScopeValidationEnabled` â€” validates the whole graph with `ValidateScopes`/`ValidateOnBuild`.
  - `AppDbContext_IsScoped_NotSharedBetweenScopes` â€” same instance inside a scope, different instance between scopes.
  - `ContextFactory_IsScoped_NotSharedBetweenScopes` â€” factory differs per scope and returns a fresh context every call.
- Build **0 errors / 21 warnings** (all `NU1701`), Unit **84/84**, Integration **44/44**, `dotnet ef migrations has-pending-model-changes` clean.
- Application starts with scope validation enabled and no exceptions logged (30 s run).

## [Unreleased] â€” 2026-10-29 (G1.1 camera discovery/fixes)

### Fixed

#### G1.1 â€” Camera discovery (E6)
- **CameraDeviceEnumerator** (`DzentoPOS2/Services/Barcodes/CameraDeviceEnumerator.cs`): replaced broken DirectShow GUID/COM enumeration with **AForge `FilterInfoCollection(FilterCategory.VideoInputDevice)`** (commits `5a05f17`, `54794df`). Enumerates on demand; returns empty safely when subsystem missing.
- **BarcodeScannerOverlay** (`DzentoPOS2/Forms/Sales/BarcodeScannerOverlay.cs`): re-enumerates on open/refresh with **Refresh** button, persists selection by moniker with fallback, shows no-device message with hint `"Start DroidCam Client first; the app must match its 32/64-bit filter"` including process bitness (64/32-bit), improved error reporting.
- **SettingsForm** (`DzentoPOS2/Forms/Settings/SettingsForm.cs`): Refresh re-enumerates cameras and re-selects by moniker.
- **BarcodeCameraService** (`DzentoPOS2/Services/Barcodes/BarcodeCameraService.cs`): supports **IP/MJPEG** sources (`http://`/`https://`) via AForge `MJPEGStream` through same pipeline; `IVideoSource` abstraction; robust stop/dispose.
- **Verification**: build 0 errors / 21 warnings (all NU1701), Unit **84/84**, Integration **41/41**, `dotnet ef migrations has-pending-model-changes` clean.

## [Unreleased] â€” 2026-10-27 (retarget to .NET 9 for Visual Studio 2022)

### Changed

#### Framework

- Retargeted every project back to **.NET 9**: libraries to `net9.0`, the WinForms app and `EZControls` to `net9.0-windows`. Removed `global.json` so the build uses the machine-wide SDK.
- Aligned all Microsoft packages (EF Core, Sqlite, Design, Tools and `Microsoft.Extensions.*`) to **9.0.22**.
- **End users need the .NET 9 Desktop Runtime** (`Microsoft.WindowsDesktop.App`) until the installer bundles it.
- Added a repository-scoped `NuGet.config` (nuget.org, cleared package source mapping): a machine-level `packageSourceMapping` that maps only the pattern `"new"` silently downgrades unavailable packages to cached ones, which blocked downloading 9.0.22.

#### Why not .NET 10

- Visual Studio 2022 (17.14, MSBuild 17.14.10) cannot load the .NET 10 SDK: SDK `10.0.403` declares `minimumMSBuildVersion = 18.0.0` (Visual Studio 2026). MSBuild falls back to the machine-wide .NET 9 SDK, which reports `NETSDK1209`; the "use Visual Studio version 17.16 or higher" text is a stale placeholder. Reproduced outside the IDE â€” a toolchain constraint, not a repo bug. Details in `docs/net9-retarget.md`.

#### Carried over from the .NET 10 attempt (still required on .NET 9)

- `WFO1000` (WinForms code-serialization analyzer, error by default on net9+) disabled via `.editorconfig` for the two programmatically-consumed control libraries (`EZControls`, `DzentoPOS2.WinForms`).
- `EZComboBox.SelectedValue` annotated `[DisallowNull]`.
- Cash-session linking columns (`CashTransactions.CashSessionId`/`SourceId`/`SourceType`, `SalesInvoices.CashSessionId`) defined in the baseline `InitialCreate` migration: EF Core **9** also rebuilds a SQLite table for `AddForeignKey` by copying the full target column list, so fresh databases failed with `no such column`.

### Verified

- Clean rebuild **0 errors / 21 warnings** (all `NU1701`, baseline unchanged), Unit **84/84**, Integration **41/41**.
- `has-pending-model-changes` clean; a fresh database applies all 9 migrations (`Done.`, 56 tables).

## [Unreleased] â€” 2026-10-27 (.NET 10 retarget) â€” superseded

### Changed

#### Framework

- Retargeted every project from .NET 8 to **.NET 10 (LTS)**: libraries to `net10.0`, the WinForms app and `EZControls` to `net10.0-windows`. Added `global.json` pinning SDK `10.0.403` with `rollForward: latestFeature`.
- Aligned all Microsoft packages (EF Core, Sqlite, Design, Tools and `Microsoft.Extensions.*`) to **10.0.14**.
- **End users now need the .NET 10 Desktop Runtime** (`Microsoft.WindowsDesktop.App`) until the installer bundles it.

#### Fixes required by .NET 10

- `WFO1000` (WinForms code-serialization analyzer, error by default on net9+) disabled via `.editorconfig` for the two programmatically-consumed control libraries (`EZControls`, `DzentoPOS2.WinForms`).
- `EZComboBox.SelectedValue` annotated `[DisallowNull]` to match the .NET 10 non-nullable annotation on `ListControl.SelectedValue`.
- Added the cash-session linking columns (`CashTransactions.CashSessionId`/`SourceId`/`SourceType`, `SalesInvoices.CashSessionId`) to the baseline `InitialCreate` migration: EF Core 10's SQLite `AddForeignKey` table rebuild copies the full column list, so fresh databases failed with `no such column`. Legacy databases are unaffected (`LegacySchemaRepair` remains the idempotent path).
- Regenerated `AppDbContextModelSnapshot.cs` for EF Core 10 â€” only the `ProductVersion` annotation changed (`8.0.27` â†’ `10.0.14`); a probe migration proved the generated Up/Down are empty (no schema change) and no existing migration was edited or renamed.

#### Package audit

- No vulnerable packages. Only `xunit 2.9.3` is flagged as deprecated (`Legacy` â†’ xunit.v3); the migration to xunit v3 is deferred.
- Kept `System.Data.SqlClient`: `System.Windows.Forms.DataVisualization.dll` references it directly while its nuspec declares no dependency, so removing it breaks the Dashboard charts at runtime.
- Licence findings needing a business decision: **AutoMapper 16** (RPL-1.5 or paid commercial licence), **QuestPDF** (MIT below US $1M revenue, else a paid tier) and **AForge.Video.DirectShow** (LGPL-3.0). Details in `docs/net10-package-audit.md`.

### Verified

- Clean rebuild **0 errors / 21 warnings** (baseline unchanged; all `NU1701`), Unit **84/84**, Integration **41/41**.
- `has-pending-model-changes` clean; a fresh database and three legacy starting states upgrade without data loss through the real startup path (`docs/net10-verification.md`, `docs/net10-smoke-run.md`).

## [Unreleased] â€” 2026-10-27 (cash drawer, discount ceiling, localized errors)

### Added

#### Cash-drawer dashboard, live totals and enforcement (F3, E3)

- `ISalesEventPublisher` / `SalesEventPublisher` (singleton, in-process): `SaleCompleted`, `SaleVoided` and `CashMovementAdded` are raised **after** the database transaction commits, so any subscriber that reads the database observes the committed rows. Handlers are isolated (one failing subscriber cannot break the sale) and subscribers own their lifetime (unsubscribe on dispose).
- `CashSessionOpenDialog` (opening policy modal) and a rebuilt `CashSessionForm` KPI dashboard (8 `KpiCard`s, 4â†’2â†’1 column reflow, RTL, dark mode, `RegionalContext` formatting). Both are designer-first (layout in `<Form>.Designer.cs`, logic in `<Form>.cs`, runtime `ApplyTheme` / localized text).
- The main shell status bar gains a live cash-drawer chip (open/closed + previous-business-day warning); `SalesForm` shows an "open the drawer first" banner and gates the checkout button.
- `GetSessionSummaryAsync` is the single source of truth for the drawer totals and is reused by `CloseSessionAsync` (idempotent: a second close throws `CodedDomainException(SessionAlreadyClosed)`).
- Migration `20261027164243_E3_UniqueOpenSessionIndex` adds the filtered unique index `UX_CashSessions_OpenPerCashbox` (`"Status" = 0 AND "IsDeleted" = 0`) â€” it previously existed only in the model snapshot.
- `CashSessionPolicy.IsFromPreviousBusinessDay` (local business day) with unit coverage.

#### Discount ceiling for both modes + seeded setting (F2b / Part 0)

- `sales.max_discount_percent` is now enforced for **both** discount modes by comparing the effective percentage (`DiscountAmount / Subtotal * 100`) of the calculator output.
- `AppSettingKeys` + idempotent `AppSettingSeeder` seed the `sales.max_discount_percent` setting.
- `CodedDomainException` / `DomainErrorCodes` + `ExceptionFormatter.LocalizeCode` localize domain and validation failures through stable codes; the printed receipt now takes its labels from `AppStrings`.

#### Upgrade-safe unique open-session migration + localized compact KPI suffixes (F3b)

- `LegacySchemaRepair` gains a pre-migration step that closes duplicate open cash sessions before the unique open-session index is created: for every cashbox it keeps the **newest** session by `OpenedAtUtc` and closes the rest with `Status = Closed`, `ClosedAtUtc = max(OpenedAtUtc, latest linked movement/sale time)`, unchanged balances and an appended note `"Auto-closed by upgrade (duplicate open session)"`. It runs in one transaction, is idempotent (skipped once `UX_CashSessions_OpenPerCashbox` exists) and logs every affected session/cashbox. It never deletes rows and never touches invoices or cash movements.
- `KpiCompactSuffixes` (Shared) carries the localized thousand/million/billion short forms; `KpiValueFormatter.Compact` no longer hardcodes `K/M/B` and uses the supplied suffixes (falling back to the invariant ones only when none is given). New `AppStrings` keys `KpiSuffixThousand` / `KpiSuffixMillion` / `KpiSuffixBillion` in base + `ar` + `en` + `fr` + `AppStrings.Designer.cs` (parity diff 0); `KpiCard` passes them in.

### Changed

- `CashSessionService` aggregates the session's cash totals in memory (SQLite cannot `Sum` a `decimal` server-side); the summary exposes `CashSales`, `CashRefunds`, `PayIns`, `PayOuts`, `Expenses`, `TotalSales`, `NonCashSalesByMethod`, `InvoiceCount` and `VoidCount`.
- `SaleService.VoidAsync` only reverses **cash** that was actually taken, so a voided card sale never moves the drawer.

### Fixed

- Cash sale â†’ exactly one linked `CashTransaction`; a card sale appears only in the non-cash split; a partial payment counts only the cash actually paid; a void nets the drawer back to zero.
- `CashTransaction` / `SalesInvoice` â†’ `CashSession` foreign keys are `Restrict` (migration `20261023140849_E3_CashSessionLinking`).
- Startup no longer aborts with `SQLite Error 1: 'no such index: IX_CashSessions_CashboxId'`: the E3 unique-index migration now drops the old index with `DROP INDEX IF EXISTS "IX_CashSessions_CashboxId"` (and `Down()` recreates it with `CREATE INDEX IF NOT EXISTS`), so a missing index can never crash the upgrade path again.
- Startup no longer aborts with `SQLite Error 1: 'index UX_CashSessions_OpenPerCashbox already exists'`: the same migration creates the filtered unique index with `CREATE UNIQUE INDEX IF NOT EXISTS` (and `Down()` drops it with `DROP INDEX IF EXISTS`), so it is a harmless no-op when the index is already present (for example from an earlier incarnation of the migration whose history row is gone).
- A database that allowed several open sessions per cashbox (pre-`E3_UniqueOpenSessionIndex`) now upgrades without error: the duplicate sessions are auto-closed by the repair step above instead of failing the `CREATE UNIQUE INDEX`.

### Verification

- `dotnet build DzentoPOS2.sln`: **0 errors**, 21 warnings (pre-existing `NU1701`).
- `dotnet test DzentoPOS2.sln`: **Unit 84 passed / Integration 41 passed / 0 failed** (F3 baseline: Unit 79 / Integration 37; Part 0: Unit 76 / Integration 26).
- 4-resx + `AppStrings.Designer.cs` key parity: **376 keys each, 0 diff**.
- `dotnet ef migrations has-pending-model-changes`: **No changes** (model snapshot and migration Designer stay consistent).

## [Unreleased] â€” 2026-10-23 (cash sessions + dashboard performance)

### Added

#### Daily cash sessions (C.1â€“C.2)

- `CashSession` entity + `CashSessionStatus` enum, `CashSessionConfiguration` (decimal(18,2) balances, indexes, restrict FK to `Cashbox`) and migration `20261023082755_AddCashSessions`.
- `ICashSessionRepository` / `CashSessionRepository` and `ICashSessionService` / `CashSessionService`: one open session per cashbox; closing computes the expected balance from `CashTransaction` rows since `OpenedAtUtc` (signs from `MovementType`), stores `ExpectedBalance` / `Difference`, closes the session and updates `Cashbox.CurrentBalance`.
- `CashboxSeeder` creates a default cashbox for the first branch and runs from `DbInitializer`.
- `CashSessionForm` (cashbox picker, opening/closing balances, open/close buttons, session grid and a localized status line) plus the `btnCashSessions` sidebar entry and DI registration.
- Localization: `CashSessions`, `Cashbox`, `OpeningBalance`, `ClosingBalance`, `ExpectedBalance`, `Difference`, `OpenCashbox`, `CloseCashbox`, `NoOpenSession`, `SessionAlreadyOpen`, `SessionOpen`, `SessionClosed`, `OpenedAt`, `ClosedAt` in base/`ar`/`en`/`fr` and `AppStrings.Designer.cs`.

### Changed

#### Dashboard summary uses 19 DB commands instead of ~34 (D.1)

- KPI aggregates per table are computed in one grouped query (`GroupBy(_ => 1)` + conditional `Sum`); monthly sales/purchases reuse those grouped reads; the six-month sales read also feeds the 30-day trend; category names are joined into the distribution query.
- Measured with an EF Core `DbCommandInterceptor` in the harness: **19 commands** for `GetSummaryAsync` (target â‰¤ 22).

### Fixed

- `EnableSensitiveDataLogging` / `EnableDetailedErrors` are now compiled only in `DEBUG` (`DzentoPOS2.Data\DependencyInjection.cs`, D.2), so release builds no longer log parameter values.
- Removed two duplicate `Click` handlers in `MainForm.WireNavButtons` that dereferenced the sidebar buttons before they were created.

### Verification

- `dotnet build DzentoPOS2.sln`: **0 errors** (only pre-existing `NU1701` package notices).
- Headless UI harness: **91 passed / 0 failed** â€” previous sections plus `C.1.0â€“C.1.6`, `C.2.1â€“C.2.5`, `D.1.1â€“D.1.2`, `D.2.1` and `D.3.1â€“D.3.2`.
- DPI review (D.3): `AutoScaleMode=Font`, `DeviceDpi > 0` and `ApplicationHighDpiMode=PerMonitorV2`; no Apple-specific assets exist in this Windows-only solution.

## [Unreleased] â€” 2026-10-23 (categories & units management)

### Added

#### Product categories and units CRUD (B.1â€“B.4)

- `IProductCategoryService` / `ProductCategoryService` and `IProductUnitService` / `ProductUnitService` provide `GetAllAsync`, `GetByIdAsync`, `SearchAsync`, uniqueness checks (`ExistsByNameAsync`, plus `ExistsBySymbolAsync` for units), `GetBaseUnitsAsync` (units), `CreateAsync`, `UpdateAsync` and `DeleteAsync`.
- New `CreateProductCategoryDto`, `UpdateProductCategoryDto`, `CreateProductUnitDto`, `UpdateProductUnitDto` and FluentValidation validators (category name â‰¤150 / description â‰¤500, unit name â‰¤100 / symbol â‰¤20).
- New `ProductCategoriesForm` + `ProductCategoryEditDialog` and `ProductUnitsForm` + `ProductUnitEditDialog`, with search, add/edit/delete and an active flag for categories.
- Two new sidebar entries (`btnCategories`, `btnUnits`) under Suppliers, navigating to the new forms, wired in `MainForm` and registered in `Infrastructure\DependencyInjection.cs`.
- Localization: `Categories`, `CategoryName`, `Units`, `UnitName`, `Symbol`, `BaseUnit`, `AddCategory`, `EditCategory`, `DeleteCategoryConfirm`, `AddUnit`, `EditUnit`, `DeleteUnitConfirm` in base/`ar`/`en`/`fr`, with matching `AppStrings.Designer.cs` properties.

### Fixed

#### Category/unit update threw "another instance with the same key is already being tracked"

- Same root cause as the A.1 product fix: the update/delete paths read the entity through the no-tracking repository and then attached a second instance. Both services now read the entity **tracked** from `AppDbContext` before updating/deleting.
- Unit `Symbol` is a unique column in the database, so `CreateAsync`/`UpdateAsync` now validate it up front and throw `InvalidOperationException` instead of surfacing a raw `SQLite Error 19: UNIQUE constraint failed`.

### Verification

- `dotnet build DzentoPOS2.sln`: **0 errors** (only pre-existing `NU1701` package notices).
- Headless UI harness: **74 passed / 0 failed** â€” P1â€“P8, DM.1â€“DM.3, A.1â€“A.4 and the new B section (`B.1.1â€“B.1.4`, `B.2.1â€“B.2.4`, `B.3.1`, `B.4.1â€“B.4.4`).

## [Unreleased] â€” 2026-10-22 (hotfixes: product tracking, settings layout, sidebar, product names)

### Fixed

#### Product update/delete threw "another instance with the same key is already being tracked" (A.1)

- Root cause: `ProductService.UpdateAsync` / `DeleteAsync` and `ExpenseService.UpdateAsync` / `DeleteAsync` read the entity with `GetByIdAsync` (which uses `AsNoTracking` on the shared `DbContext`) and then called `Update`/`Remove` on a detached copy while the context could still track another instance with the same key.
- Fix: the services now read the entity **tracked** from `_context` (`FirstOrDefaultAsync` with `Include(Category, Unit)` for updates, plain tracked read for deletes) and operate on it directly.
- Measured by headless harness `A1.1`: create a product then update it twice on the same service instance â‡’ final name `uihost-product-2`; before the fix this threw.

#### Settings cards clipped fields at narrow widths and on language change (A.2)

- Root cause: `SettingsCard` reported its height from `TableLayoutPanel.GetPreferredSize`, which under-measured wrapped captions and left the last field cut off the card; captions were never re-measured after a language switch.
- Fix: `SettingsCard` now measures every field caption at the current width (title/hint rows, stacked label rows, full-width control rows) and sets absolute row styles for every stack row; `RequiredHeight` sums the real row heights. `SettingsForm` re-layouts the cards when the language changes.
- Measured by headless harness `A2.1`: 0 clipped fields at 375/768/1440 px in Arabic, French and English.

#### Dashboard and Sales buttons both showed the accent color after login (A.3)

- Root cause: `MainForm.Designer.cs` set `btnDashboard.BackColor = AppColors.Primary` and `ApplySidebarButtonHover` reapplied the accent, while nothing repainted the sidebar from the single active-button state after `NavigateTo<SalesForm>(BtnSales)`.
- Fix: the designer entry is now `AppColors.Surface`; the hover leave handler no longer forces the accent back on `btnDashboard`; `MainForm_Load` calls `ApplyShellTheme()` after the startup navigation so exactly `_activeSidebarButton` (sales) carries the accent.
- Measured by headless harness `A3.1`: 9 sidebar buttons, exactly one accent button (`BtnSales`).

#### Category / Unit columns empty in the product grid (A.4)

- Root cause: `ProductRepository.GetAllAsync` / `FindAsync` returned products without `Include(Category, Unit)`, so `CategoryName` / `UnitName` stayed blank in `ProductsForm`.
- Fix: both reads now eager-load `Category` and `Unit` with `AsNoTracking()`.
- Measured by headless harness `A4.1`: `GetAllAsync` returns products with non-empty `CategoryName` and `UnitName`.

### Verification

- `dotnet build DzentoPOS2.sln -t:Rebuild -v:m -nologo`: **0 errors** (only pre-existing `NU1701` package notices).
- Headless UI harness: **61 passed / 0 failed** â€” P1â€“P8, DM.1â€“DM.3 and A.1â€“A.4.

## [Unreleased] â€” 2026-10-22 (dark mode: tab strips + KPI badges)

### Fixed

#### Tab strips stayed light in dark mode (DM.1 / DM.3)
- Root cause: WinForms hard-codes `TabControl.BackColor` to `SystemColors.Control` (the property setter is empty in .NET 8), so a themed tab strip kept the light system colour while every other surface switched to the dark palette. `DrawItem` is also clipped to the tab rectangles, so it cannot paint the empty strip next to the last tab.
- New `Themes\TabsStyler.cs` switches a `TabControl` to `TabDrawMode.OwnerDrawFixed` and paints it with the active palette: the strip background uses `AppColors.Background` (from a `Paint` handler), the selected tab uses `Surface` with a 3px `Primary` accent, the other tabs use `SurfaceAlt` with a `Border` line, and the captions are rendered with `TextRenderer` in `Ink` / `InkMuted`.
- `ThemeManager.ApplyToControl` applies the styler to every `TabControl` of a themed container, and the point-of-sale, products and reports pages apply it explicitly for their own tabs.
- Measured by the headless harness with `DrawToBitmap` (pixel sampling, not the unusable property): `SalesForm`, `ProductsForm` and `ReportsForm` header / strip / page all render `(17,24,39)` in dark mode (before: light system colour).

#### KPI badge chips stayed pastel in dark mode (DM.1)
- `KpiCard` painted the icon badge from `Color.White` blended with the card accent, which produced a pale chip that broke the dark surface. The badge now blends against `AppColors.Surface` at 30% in dark mode and keeps the 15% white tint in light mode (new `Blend` helper).
- Measured: the dark-mode surface sweep reports **0 light surfaces** on every page (previously `KpiCard` badges on Dashboard / Inventory / Reports).

### Verification
- `dotnet build` of the UI harness against the solution: **0 errors** (only the pre-existing `NU1701` package notices).
- Headless harness: **57 passed / 0 failed** â€” includes `DM.1` (no light surface on all 12 pages), `DM.2` (no dark-on-dark captions) and `DM.3` (rendered tab strips dark on Sales / Products / Reports).
- Temporary screenshot/debug code (`PrintWindow`, `CaptureTabStrip`) was removed from the harness after the fix; the pixel checks are self-contained.

---

## [Unreleased] â€” 2026-10-22 (settings layout + point of sale fixes)

### Fixed

#### Customer drop-down in the point of sale (P3.3)
- `SalesForm` read the selected customer through `ComboBox.SelectedValue`, but the drop-down is filled by hand (`Items.Add`), so `SelectedValue` stayed `null` and the selected customer was never carried into the sale line. The identifier is now read from the selected item (`SelectedItem is CustomerItem selectedCustomer ? selectedCustomer.Id : Guid.Empty`).
- Measured: every item is a `CustomerItem`, the id is readable from `SelectedItem`, `SelectedValue` is `null` (proving the old code path was dead), and no non-comment line of `SalesForm.cs` mentions `SelectedValue` any more.

#### Supplier drop-down in the purchase dialog
- `PurchaseEditDialog` used the same dead `SelectedValue` pattern with a manually filled drop-down, so a purchase could never be saved. A `SelectedId(ComboBox)` helper now resolves the id from the selected item's `Id` property.

#### User avatar in the shell
- `iconPictureBox1` in `MainForm` was anchored `None` at `Y = -3`, so it was clipped by the top edge of the user panel. It is now anchored `Top | Left` at `(6, 2)`.

#### Barcode scanner crashed with "Parameter is not valid." (P6.4)
- Opening the camera scanner threw `System.ArgumentException: 'Parameter is not valid.'` from `path.AddArc(0, 0, r, r, 180, 90)` in `UIHelper.ApplyRoundedRegion` (the rounded region of the scanner preview).
- Root cause: the radius was computed as `Math.Min(radius, Math.Min(width, height) / 2)` with **integer division**. As soon as the preview host was one pixel high (or wide) while the layout was still running, the radius became `0` and GDI+ rejects an arc with an empty ellipse rectangle. The old guard only skipped a *zero* width/height, not a size of `1`.
- The radius is now clamped the same way in every rounded-region helper and a `0` radius falls back to the rectangular region instead of building a degenerate path: `UIHelper.ApplyRoundedRegion` (scanner preview + dashboard cards), `RadiusControl.Apply` (every themed button, the login button, the KPI badge, the loading overlay), `KpiCard.ApplyRegion` and `RegionHelper.GetRoundPath`. The previous region is also disposed instead of being leaked.
- Verified by the headless harness (`P6.4`): a panel with an empty height, a `1x1` panel and a normal panel are all handled without an exception, and the region is still applied on the normal one.

#### Settings page at small widths (P5.2 / P8.1)
- The settings page was a `FlowLayoutPanel` of auto-sized cards: at 375px the cards kept their designer width and overflowed horizontally, and the logo picture (96px) was taller than the row it was placed in, producing a vertical overflow.
- `SettingsForm` now lays the cards out in a `TableLayoutPanel` grid: two columns when the page is at least 980px wide, one column below that, with every card sharing the same column width and the row heights derived from the measured card heights.
- `SettingsCard` no longer relies on `AutoSize`: it owns an inner `TableLayoutPanel`, switches between a side-by-side and a stacked (label above field) field layout below 560px of inner width, and reports its required height so the grid can size the rows.
- The branding card places the logo in a fixed 96px row with the picture sized to fit, so nothing overflows.

#### Product search hidden under the basket in the point of sale (P3.5 / P3.6 / P3.7)
- Root cause: in `SalesForm` the product panel was added to the sale tab **after** the basket panel, and WinForms docks the **last added** control first. The product panel (`Dock = Fill`) therefore took the whole tab width while the basket (`Dock = Right`, 420px) was painted on top of it, covering the right part of the product search field and the whole barcode button.
- The sale tab is now a `TableLayoutPanel` (`_saleLayout`) that owns both sides: the product panel and the basket are two real columns, so they can no longer overlap. The basket is a `TableLayoutPanel` (`_cartLayout`) as well: customer row (34px), the cart grid (grows), the totals block and the action buttons.
- The layout adapts to the available width: side by side (basket column `clamp(38%, 300, 420)`) when the page is at least 780px wide, stacked (basket below the product list) below that. The recomputation is deferred by one message-loop pass (`OnSaleTabResized` â†’ `BeginInvoke`) because `Resize` fires before the new size is applied.
- The totals block is measured against the padding of its table (`TotalsPadding = 6`), so the last total row no longer overflows its cell (measured before the fix: label `bottom = 156` inside a `150` high table).
- The cart grid columns and the product grid columns now stretch (`AutoSizeMode = Fill`) instead of keeping fixed widths, so the grids fit the new panel widths.
- The basket title is a real resource (`AppStrings.Cart`: `Shopping cart` / `ط³ظ„ط© ط§ظ„ظ…ط´طھط±ظٹط§طھ` / `Panier d'achat`) and is refreshed in `LoadText()` when the language changes.

### Changed

- `SettingsForm.MinimumSize` is now `320 x 420` (was `720 x 520`) so the page can be hosted inside the narrow shell without being forced to a desktop size.
- `DzentoPOS2.Data\DependencyInjection` creates the folder of the SQLite file when it is missing, so a fresh installation starts without any manual step.

### Decision

- The built-in administrator password stays **forced on every startup**: `AdminSeeder` rewrites the stored hash to the built-in default whenever it no longer verifies against it. Changing the administrator password from the **Users** page therefore has no lasting effect â€” confirmed as the intended behaviour (the account remains the documented `admin` / `admin` recovery entry).

### Verification
- `dotnet build DzentoPOS2.sln -t:Rebuild -v:m -nologo`: **0 errors** (21 warnings, all pre-existing `NU1701` package-compatibility notices; the last `CS1998` in `SalesForm` was removed as well).
- Headless UI harness: **41 passed / 0 failed** â€” P1 (version, currency, language round-trip, credentials, retired default password absent from the sources, copyright strip at form level), P2 (sidebar scroll, user block at 220px/100px, RTL, avatar), P3 (POS opens by default, add-customer button, customer id, calculator, **product search and barcode button visible above the basket, no overlap between the two sides, stacked basket on a 420px page**), P5 (card heights, uniform card width and no overflow at 375 / 768 / 1440), P6 (rounded region guard of the scanner preview) and P8 (settings page, shell and settings hosted in the shell, all at 375 / 768 / 1440).
- Auth harness (real DI + SQLite): `admin` / `admin` succeeds, the retired default no longer does, a legacy clear-text password is accepted once and upgraded to a `$2a$11$` hash.

---

## [Unreleased] â€” 2026-10-21 (login fixes)

### Fixed

#### Login screen layout (broken fields / button)
- The inner field `TableLayoutPanel` in `LoginForm` had **no column style**, so its single column fell back to `AutoSize`; the controls kept their designer width (706px) and overflowed the 518px centre column (measured at `X=-188, Width=706`). An explicit `ColumnStyle(SizeType.Percent, 100F)` was added, so every field and the button now measure exactly `X=0, Width=518` inside the column.

#### Login credentials (sign-in always failed)
- The runtime database held an administrator whose password was the old default, while `AdminSeeder` created the account with the built-in default; because the seeder skipped existing admins, sign-in with the built-in default failed. `AdminSeeder` now **ensures** the built-in administrator's password equals the built-in default on every startup: it rewrites the stored hash only when it no longer verifies against the default. Verified against both databases the app can open (`bin/.../Database/DzentoPOS.db` and `DzentoPOS2/Database/DzentoPOS.db`): `admin` / `admin` returns `Success = true` (`Admin@123` no longer does).

#### Database location (independent of the working directory)
- The SQLite connection string used a **relative** `Data Source`, so the database file depended on the process working directory. Launching from Visual Studio, from the output folder or from a shortcut could open (or create) a different file, which is why the sign-in kept failing while the database looked correct.
- `DzentoPOS2.Data\DependencyInjection.cs` now resolves a relative `Data Source` against the application base directory (`AppContext.BaseDirectory`), so the application always uses `Database\DzentoPOS.db` next to the executable.
- Both existing databases were emptied and re-seeded from scratch (`MigrateAsync` + seeders). Verified: `users = 1`, the stored hash is a valid BCrypt value, `admin` / `admin` verifies against it, and `Admin@123` does not.

#### Password hashing (accounts created before hashing)
- Root cause found: `BCrypt.Net.BCrypt.Verify` **throws** `SaltParseException` when the stored value is not a valid BCrypt hash (a legacy clear-text password), instead of returning `false`. The exception propagated out of `PasswordHelper.Verify`, so any account created before hashing was introduced could no longer sign in.
- `PasswordHelper.Verify` now treats a non-hashed / malformed stored value as "not matching" (returns `false`, never throws), and a new `PasswordHelper.IsHashed` helper detects BCrypt-style values.
- `AuthService.LoginAsync` migrates such accounts on first successful sign-in: when the stored value is not a hash and equals the entered password, it re-hashes the password and saves it, so the account keeps working afterwards.
- Verified end to end (isolated scopes against the real SQLite database): `admin` / `admin` succeeds, a legacy clear-text password (`legacy-pass`) is accepted once and upgraded to a `$2a$11$` hash, and the next startup restores the built-in default.

### Changed

#### Login screen behaviour
- Pressing **Enter** in either the username or the password field now submits the form (the custom `EZButton` is not an `IButtonControl`, so the form's `AcceptButton` could not be used).

### Verification
- `dotnet build DzentoPOS2.sln -t:Rebuild -v:m -nologo`: **0 errors** (only the pre-existing `NU1701` package-compatibility warnings).
- Headless layout harness (instantiating `LoginForm` against the built assembly) confirms the field column now fits the centre area with no overflow.
- Auth harness (real DI + SQLite) confirms `admin` / `admin` returns `Success = true`.

---

## [Unreleased] â€” 2026-10-21 (suppliers page)

### Added

#### Suppliers page (P4)
- `SuppliersForm` (`Forms/Suppliers`): supplier table with search, and full add/edit/delete driven by `ISupplierService` (`GetAllAsync` / `SearchAsync` / `CreateAsync` / `UpdateAsync` / `DeleteAsync`). The grid shows name, phone, country, city, address, balance and status. Opening the page issues a single list query.
- `SupplierEditDialog` (`Forms/Suppliers`): create/edit dialog with name validation, phone, country, city, address and active flag; on create it exposes the new `SupplierDto` so callers can react immediately.
- Suppliers entry registered in DI (`AddTransient<SuppliersForm>()`) and wired into the shell as a sidebar button (`btnSuppliers`, `Truck` icon) next to **Customers**, navigating to `SuppliersForm` and following the active theme / RTL / collapse behaviour like every other sidebar item.

### Verification
- `dotnet build DzentoPOS2.sln -t:Rebuild -v:m -nologo`: **0 errors**; the only warnings are the pre-existing `NU1701` package-compatibility notices (AForge / OpenTK / SkiaSharp).
- Localization keys `Suppliers`, `SupplierName`, `AddSupplier`, `EditSupplier`, `DeleteSupplierConfirm`, `Country`, `Balance`, `StatusFilter`, `Address` verified present in all four resource files (base/ar/en/fr).

### Changed

#### Localization base file is English (P7)
- `AppStrings.resx` (the neutral/base fallback) now carries the **English** values instead of the Arabic ones: all 289 entries were aligned to their English counterparts, so the base file contains **0 Arabic values**. `AppStrings.ar.resx` is untouched and still holds the Arabic translations.
- Key parity re-verified across the four files: **289 keys each**, 0 differences between base/ar/en/fr.

### Fixed

#### Navigation
- `NavigationService.Navigate<TForm>` resolves the target page **before** disposing the current one, so a DI failure (or any exception while hosting the page) shows an `AppMessageBox` error and leaves the previous page in place instead of blanking the shell.

#### Barcode scanner
- `BarcodeCameraService.Stop()` now releases the camera properly: it unsubscribes the frame/error handlers, then runs `SignalToStop()` / `WaitForStop()` (the AForge way to tear down the DirectShow graph and its COM objects) with a `Stop()` fallback, and defensively calls `Dispose()` when the concrete device type implements `IDisposable`. Repeated scans no longer leak the capture graph.

### Added

#### Product barcode (P6.3)
- `Product.Barcode` (nullable, max length 64, unique index) added to the domain model and configured in `ProductConfiguration`; exposed on `ProductDto`, `CreateProductDto` and `UpdateProductDto`, mapped in `ProductService`, and editable from `ProductEditDialog` (new **Barcode** field, layout shifted accordingly).
- The sales-screen scanner now matches a scanned code against the product **barcode** first and falls back to the SKU.
- EF Core migration `20261021192318_AddProductBarcode` adds the `Products.Barcode` column + unique index `IX_Products_Barcode` (and the pending `SalesInvoiceItems.Description` column from the sales-item work). It is applied automatically at startup through `context.Database.MigrateAsync()`.

---

## [Unreleased] â€” 2026-10-21 (settings, branding, regional, barcode)

### Added

#### Settings page (store identity, regional, appearance, printing, barcode)
- `SettingsForm` rewritten as a responsive card-based page with six cards: **Store identity** (name + logo import/clear), **Regional** (language, country, currency, time zone), **Appearance** (light/dark), **Printing** (printer, paper size, orientation, receipt width, test print), **Barcode scanning** (camera device + enable flag) and **About**. Every field is a themed control and the page is saved through `ICurrentSettingsService`.
- `AppSettings`, `ICurrentSettingsService`/`CurrentSettingsService` and `IUserPreferencesService`/`UserPreferencesService` added: preferences and store configuration now live in a single `user-settings.json` document.
- `RegionalCatalog` (currencies, countries, time zones) and `RegionalContext` (runtime currency/country/time-zone holder) added to the shared layer; `RegionalContext.ToUtcSafe` converts a local timestamp without throwing on daylight-saving gaps.
- `StoreBranding` service: imports the store logo (scaled to 640px, stored as `branding/store-logo.png`) and always hands out a clone so picture boxes never share one image instance.

#### Store identity in the shell
- `MainForm` shows the configured store name under the logo in the sidebar (hidden automatically when the sidebar is collapsed) and swaps in the custom logo once one is imported.
- `LoginForm` shows the store name and the custom logo on the brand panel.

#### Barcode scanning
- `BarcodeCameraService` captures frames with AForge and decodes them with ZXing (inverted/rotated fallbacks through `DecodingOptions`) on a background timer, so the UI never freezes.
- `CameraDeviceEnumerator` enumerates DirectShow video inputs through COM interop (`VideoCaptureDevice` monikers).
- `BarcodeScannerOverlay`: modal scanner window with a live preview, camera switching, a beep on success and ESC/close handling.
- `SalesForm` gained a barcode button next to the product search; a scanned code is matched against the product SKU and the item is added to the cart.

### Changed
- Money is rendered through `RegionalContext.FormatMoney` in `ProductsForm` and `SalesForm`, and the grid headers show the active currency code.
- `DashboardService` derives the "today"/"this month" boundaries from the configured time zone and groups the charts by local date/month, while the database keeps storing UTC.
- Custom receipt sizes are supported through a `PaperSize` built from millimetres, and landscape output swaps the paper width/height before printing.

### Fixed
- Mojibake (U+FFFD) in `LoginForm.Designer.cs` (copyright line, "ط§ظ„ط¹ط±ط¨ظٹط©"/"Franأ§ais" labels) restored from the `HEAD` blob, and the broken dash in `ProductsForm` replaced with U+2014. The `AppStrings*.resx` resources were verified to be free of replacement characters.
- Long option lists are reachable again: the custom combo box renders **every** entry inside a scrollable list (`ToolStripControlHost` + auto-scrolling panel) instead of cutting the overflow away. Verified for 42 countries, 43 time zones and 37 currencies: 6 visible rows plus a working scrollbar.
- The combo-box dropdown is direction aware: marker, check mark and text alignment follow the layout direction (RTL).
- `System.Drawing.Printing` usage adapted to the reduced .NET 8 API surface (`PaperSize.PaperName`, read-only `PaperKind`, no `PrinterSettings.Orientation`, no `LandScapeAngle`).

### Verification
- `dotnet build DzentoPOS2.sln -t:Rebuild -v:m -nologo`: **0 errors**; the 21 remaining warnings are the pre-existing `NU1701` package-compatibility notices (OpenTK / SkiaSharp / AForge).
- Runtime harness (temporary console host referencing the WinForms assembly) confirmed: `SettingsForm` and `MainForm` construct and render, all nine combo boxes are populated, long lists scroll (`autoScroll=True`, `vScroll=True`, drop-down height capped at 192px), the store-name label and the logo are visible in the sidebar (white text pixels detected in the label band), and the drop-down reports `RightToLeft=Yes` in Arabic.

---

## [Unreleased] â€” 2026-10-21

### Added

#### Theme-aware shell & header
- `LoginForm` now implements `IThemeAware` (`ApplyTheme` + `ThemeChanged` subscription, unsubscribed on dispose). The login surface, brand panel, inputs, and the primary button follow the active theme.
- `LoginForm` light/dark toggle button (`btnIconThemMode`) is now wired to `ThemeManager.ToggleMode()` and shows a `Moon`/`Sun` icon that reflects the current mode. The button was previously dead.
- `BranchSelectorControl`, `NotificationButton`, `SettingsButton`, and `UserMenuControl` implement `IThemeAware` and re-apply their colors on every theme change.
- `MainHeaderBar.ApplyTheme()` re-applies the header combo-box style and forwards the theme to every header child control.

#### Dialogs
- `Helpers/AppMessageBox.cs` fully rewritten on top of `AppColors` + `ButtonFactory` (`ThemedButton`/`ButtonTone`): accent bar, themed title/message, and semantic button tones instead of fixed light colors.

### Changed

#### KPI cards
- `KpiCard` title is no longer clipped: `AutoSize = false` with `AutoEllipsis`, plus a new `UpdateLayout()` pass so the title and the trend badge never overlap.
- `ReportsForm` KPI panel re-sized to `Height = 288` with two absolute `132px` rows so the two KPI rows are never clipped (Dashboard/Inventory/Products layouts verified as sufficient).
- `KpiCardsModule.Colors` is now a dynamic property (`AppColors.KpiPalette`) instead of a static array captured at startup.

#### Lifecycle
- `MainForm`, `LoginForm`, `UserMenuControl`, and the header controls unsubscribe from `ThemeManager.ThemeChanged` on dispose (the `Dispose(bool)` overrides live in the generated `*.Designer.cs` files to avoid duplicate `Dispose` members).
- `DashboardForm` disposes its charts module (`_charts?.Dispose()`) on close.

### Fixed

#### Theme persistence (critical)
- `ThemeManager`'s static constructor called `SetMode(Light)`, which **saved the theme before `LoadTheme()` read it**, so the persisted preference was always overwritten with `Light` on every launch. The static constructor now calls `SetMode(ThemeMode.Light, save: false)`, `SaveTheme()` swallows IO failures, and `LoadTheme()` trims the file content and compares it case-insensitively.
- `SettingsForm` now initializes the dark-mode checkbox from `ThemeManager.Mode` and applies `ThemeManager.SetMode(...)` on save.

#### Live theme switching
- Switching the theme while the shell is open now re-themes the whole application immediately: `MainForm.ApplyShellTheme()` repaints sidebar, top bar, status bar, sidebar buttons, and the header bar, and `NavigationService` re-applies `AppColors` to the hosted page. Verified live in both directions (dark â†’ light and light â†’ dark) by pixel sampling.

#### Code quality
- `NavigationService` no longer produces a nullable warning: the resolved form is explicitly checked and the title update is guarded against disposed forms.

### Verification
- `dotnet build DzentoPOS2.sln -t:Rebuild -v:m -nologo`: **0 errors**; only the 15 pre-existing `NU1701` package-compatibility warnings (OpenTK / SkiaSharp).
- Runtime smoke test: `SplashForm` â†’ `Login` â†’ `MainForm` with no crash; theme restored from `theme.config`; live toggle verified by `PrintWindow` pixel sampling (dark: sidebar `#090E16`, content `#111827`; light: sidebar `#002731`, content `#F5F7FA`).
- RTL verified at runtime: switching the UI to Arabic mirrors the layout (sidebar moves to the right edge) and the shell re-creates accordingly.

---

## [Unreleased] â€” 2026-10-20

### Added

#### Services & Data Access (App layer)
- `ISaleService` / `SaleService` â€” sales invoice creation, listing, deletion (`DzentoPOS2.App/Services/Sales`).
- `ICustomerService` / `CustomerService` and `ISupplierService` / `SupplierService` â€” partner management (`DzentoPOS2.App/Services/Partners`).
- `IPurchaseService` / `PurchaseService` â€” purchase invoice handling (`DzentoPOS2.App/Services/Purchases`).
- All new services registered in `DzentoPOS2.App/DependencyInjection.cs`.

#### DTOs (Shared layer)
- Sales DTOs under `DzentoPOS2.Shared/Contracts/DTOs/Sales`.
- Partner DTOs under `DzentoPOS2.Shared/Contracts/DTOs/Partners`.
- Purchase DTOs under `DzentoPOS2.Shared/Contracts/DTOs/Purchases`.

#### Domain
- `SalesInvoice.Create(...)` static factory added to `DzentoPOS2.Core/Entities/Sales/SalesInvoice.cs` to support value-object construction (private-setters on `InvoiceNumber`, `Money`, `Percentage`, `Tax`, `PaidAmount`, `RemainingAmount`).

#### Forms
- `CustomersForm` + `CustomerEditDialog` â€” full CRUD for customers (`Forms/Customers`).
- `SalesForm` â€” POS-style cart with product grid, quantity, discount, tax, paid amount, complete-sale action, and an invoices tab (`Forms/Sales`).
- `PurchasesForm` + `PurchaseEditDialog` â€” purchase CRUD (`Forms/Purchases`).
- `UsersForm` + `UserEditDialog` â€” full user CRUD (create/edit) with role assignment (`Forms/Identity`).
- `RolesForm` + `RoleEditDialog` â€” role CRUD with permission checklist (`Forms/Identity`).
- `InventoryForm` â€” stock overview: 4 KPI cards, search + category/status filters, product grid, and per-warehouse stock detail (`Forms/Inventory`).
- `ReportsForm` â€” 8 KPI cards plus tabbed insight grids (top selling, best customers, active suppliers, stock alerts) driven by `IDashboardService` (`Forms/Reports`).
- `SettingsForm` â€” language selector, dark-mode toggle (wired to `ThemeManager`), remember-me, and version/about card, persisted via `IUserPreferencesService` (`Forms/Settings`).

#### Localization keys (أ—4 languages: base/ar/en/fr)
- Added `Warehouse`, `Appearance`, `About`.
- Total localization keys: **241** per language.

### Changed

#### Dynamic language switching (no restart required)
- `LocalizableFormBase` provides `LoadText()` (virtual), language subscription, and RTL/LTR direction handling via `FormLocalizationHelper.ApplyDirection`.
- Converted forms to dynamic localization: `LoginForm`, `DashboardForm`, `ProductsForm`, `ExpensesForm`, `MainForm`, `AddUserForm` (removed, see below), `ProductEditDialog`, `ExpenseEditDialog`, and the eight former placeholder forms.
- `Dashboard`: `KpiCardsModule.GetTitles()` resolves titles at call time; `RecentTransactionsModule` made instance-based; `ChartsModule` receives it via constructor; `LoadText()` disposes old controls before rebuilding.
- `AppMessageBox` hardcoded Arabic strings replaced with `AppStrings`.
- `MainHeaderBar` language selector intentionally keeps native language names (no change).

#### Theme
- `ThemeManager.LoadTheme()` is now invoked at application startup in `Program.cs`, so the persisted theme is restored on launch.
- `SettingsForm` dark-mode checkbox is initialized from `ThemeManager.Mode` and applies `ThemeManager.SetMode(...)` on save.

### Fixed

#### Critical
- Removed obsolete `CustomMessageBoxV1`; introduced local `Themes/ThemeMode.cs` and `Helpers/AppMessageBox.cs`.
- Removed duplicate `ICurrentUserService` definition.
- Completed `AuthService.LogoutAsync`.
- Fixed `System.Data.SqlClient` runtime failure caused by `System.Windows.Forms.DataVisualization` (used by `ChartsModule`): re-added `System.Data.SqlClient` 4.8.6 to `DzentoPOS2.WinForms.csproj`.

#### Designer / namespace
- `CustomersForm` moved from `Forms.Identity` to `Forms.Customers`; usings updated in `Infrastructure/DependencyInjection.cs` and `MainForm.cs`.
- `CustomersForm.Designer.cs` rewritten with correct namespace and simplified `InitializeComponent`.
- `UsersForm.Designer.cs`, `RolesForm.Designer.cs`, `InventoryForm.Designer.cs`, `ReportsForm.Designer.cs`, `SettingsForm.Designer.cs` simplified (placeholder labels removed; UI built in code-behind).
- Added missing `using DzentoPOS2.WinForms.Helpers;` in `PurchaseEditDialog.cs`.
- Corrected `AppStrings.Status` â†’ `AppStrings.StatusFilter`, and nonexistent `SelectSupplierMsg` â†’ `AppStrings.SelectSupplier`.

#### GDI / rendering
- `UIHelper.SetRoundedRegion` GDI handle fixes.
- `ShadowEngine` GDI fixes plus nullability hardening.

### Removed
- `AddUserForm` (`.cs`, `.Designer.cs`, `.resx`) â€” superseded by the new `UsersForm` + `UserEditDialog`. DI registration removed.
- Unused `MainForm.loadingOverlay` field.
- Unused `LocalizableFormBase._isFirstLoad` field.
- Unused `SettingsForm._loading` field.

### Code quality (nullability & analyzer warnings)
Resolved all C# compiler warnings to achieve a warning-free build across the solution:
- `LocalizableFormBase` â€” removed unused field.
- `LoadingOverlay` â€” non-nullable field initialization and nullable event-handler signature.
- `RecentTransactionsModule` â€” null-guarded grid column and cell-style access.
- `SalesForm` â€” converted non-awaiting async methods to synchronous; null-safe `SKU` filtering.
- `LoginForm` â€” nullable `IUserPreferencesService`, null-safe text reads and assignments.
- `ExpenseEditDialog` â€” null-safe reflection on combo items.
- `ProductService` â€” removed async from non-awaiting mapper.
- `ShadowEngine` â€” nullable cache bitmap and nullable event-handler signatures.
- `ThemeManager` â€” nullable `ThemeChanged` event and `ControlAdded` guard.
- `MainHeaderBar` â€” null-safe `SearchText`.
- `ProductsForm` â€” null-safe search term.
- `EZControls`: `EZComboBox`/`EZInputBase` `Text` override nullability via `[AllowNull]`; `EZToggleButton` inner-gap constant renamed (`Padding` â†’ `InnerGap`) to stop hiding `Control.Padding`; `EZUserMenu` unused public events annotated with `#pragma warning disable CS0067`.

### Fixed

#### Dashboard statistics (correctness)
- `DashboardService` rewritten so all figures are computed correctly against `UtcNow`:
  - **Sales & purchases** now use invoice **`Total`** (value) instead of `PaidAmount`; period comparisons for the matching prior period.
  - **Low-stock card was hard-coded to `0`**; now counts active, stock-tracked products whose summed `ProductStock.Quantity` is at or below `MinimumStock`.
  - **Profit math corrected** â€” `ActualProfit` no longer double-counts purchases. It is now `grossProfit âˆ’ operating expenses`, where `grossProfit = netRevenue (sales âˆ’ returns) âˆ’ COGS`.
  - **Date ranges** converted to half-open intervals `[start, next)` (day/month), eliminating the previous `<= now` truncation of late-day invoices.
  - **Stock alerts** now show real current stock vs. reorder level instead of presenting `MinimumStock` as the stock value.
  - Removed redundant explicit `!IsDeleted` predicates (a global soft-delete query filter already applies to all `BaseEntity` types).
- `ComputeChartData` and `ComputeInsights` now aggregate in SQL (group-by/order-by/sum pushed to the database) rather than materializing tables in memory.

#### Dashboard performance / concurrency
- Removed the unsafe `Task.WhenAll` over a single `DbContext` (EF Core does not support concurrent operations on one context); computations now run sequentially.

#### Dark mode & unified theming
- **Theme system unified**: `AppColors` now resolves dynamically from `ThemeManager`, so light and dark modes share a single source of truth. All surfaces, cards, fields, grids, and controls react correctly to theme changes.
- **Instant theme switching**: Toggling dark mode in Settings now re-applies the theme to the currently hosted form immediately (no restart required). `NavigationService` listens to `ThemeManager.ThemeChanged`.
- **KPI cards & dashboard chrome**: `KpiCard`, `UIHelper.WrapInCard`, and dashboard modules use `AppColors` (surface/text/borders) instead of hardcoded `Color.White`, rendering correctly in both modes.
- **Data grids & inputs**: `DataGridViewStyleHelper` and common input styling follow the active theme (borders, headers, selection, alternates).
- **Dialogs**: All edit dialogs now use `AppColors.Surface`.

### Changed

#### Unified design system (one source of truth)
- Added `Themes/AppColors.cs` â€” semantic palette (brand, status, neutrals, surfaces, shell tokens, disabled states, tint/hover helpers), aligned with an Apple-inspired visual identity.
- Added a curated `AppColors.KpiPalette` and refactored KPI cards so the **Dashboard** and **Reports** pages share one identical card design and color system (previously the Dashboard used an arbitrary generated rainbow palette and Reports used a separate, plain inline card).
- Reports KPIs now use the shared `KpiCard` control (icon badge, accent bar, trend indicator, value animation, hover elevation) laid out in a `TableLayoutPanel`, matching the Dashboard exactly.
- `KpiCard` is now size-responsive (anchored/docked instead of fixed absolute positions) so it renders correctly in both the 4أ—3 dashboard grid and the tighter 4أ—2 reports grid.
- Added `Themes/ButtonFactory.cs` â€” centralized button creation (`Primary`/`Success`/`Danger`/`Neutral` solids + outlined `Tool*` icon buttons) with consistent sizes, radius, typography, and hover/pressed behavior.
- Removed four duplicated `CreateToolButton` implementations (`CustomersForm`, `UsersForm`, `RolesForm`, `ExpensesForm`) in favour of `ButtonFactory`.
- Converted hardcoded save/cancel/tool buttons to the factory across dialogs and forms (`CustomerEditDialog`, `UserEditDialog`, `RoleEditDialog`, `ExpenseEditDialog`, `ProductEditDialog`, `PurchaseEditDialog`, `SettingsForm`, `SalesForm`, `PurchasesForm`).
- Tokenized previously hardcoded colors across the shell and forms (`MainForm`, `MainForm.Designer`, `NavigationService`, `HeaderColors`, `LoginForm.Designer`, `DashboardForm`, `InventoryForm`, `ProductsForm`, `ReportsForm`, `SalesForm`, `PurchasesForm`, `ExpensesForm`, `SettingsForm`) to consume `AppColors`, eliminating three conflicting primary teals.
- Standardized page backgrounds on `AppColors.Background`.

#### Responsive / high-DPI
- Enabled **Per-Monitor V2** DPI awareness via `ApplicationHighDpiMode` in `DzentoPOS2.WinForms.csproj` (was implicit `SystemAware`), for crisp rendering on high-DPI and multi-monitor setups.
- Added a `MinimumSize` to `MainForm` to prevent the shell collapsing below a usable layout.

### Build status
- `dotnet build DzentoPOS2.sln -t:Rebuild`: **0 errors, 0 C# warnings**.
- Application verified to start successfully (with Per-Monitor V2 DPI awareness).

---

## Notes
- The SQLite connection string is relative (`Data Source=Database/DzentoPOS.db`). Always launch the executable with its own output folder as the working directory; launching from another directory causes `SQLite Error 14: unable to open database file` (environmental, not a code defect).
- `DzentoPOS2.UnitTests` and `DzentoPOS2.IntegrationTests` are currently empty placeholder files (no test projects to run).

## [Unreleased] - 2026-11-04 (fix/ui-kpi-sidebar-filters Phase 1)
- Phase 1: KpiCard RTL rebuild, cached fonts, responsive grid; KpiValueFormatter Latin digits; tests ar/ar-DZ/ar-SA.

## [Unreleased] - 2026-11-04 (Phases 3-7 partial)
- Phase 3: sidebar single button factory, remove categories/units, centralized collapse state.
- Phase 4: ProductsHubForm tabs.
- Phase 5: settings translations + 812px stacked.
- Phase 6: barcode scanner preview fix + translated hint.
- Phase 7: store expenses not attributed to customer.

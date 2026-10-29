# .NET 10 and EF Core 10 breaking changes (P2 / Step 3)

> **Historical (reverted).** This documents the .NET 10 retarget, which was reverted because Visual Studio 2022 cannot build `net10.0`. The solution now targets .NET 9 — see [`net9-retarget.md`](net9-retarget.md). Items 1 (WFO1000) and 3 (SQLite `AddForeignKey` rebuild) still apply on .NET 9.

Sources reviewed:

- <https://learn.microsoft.com/dotnet/core/compatibility/10.0> (.NET 10 index)
- <https://learn.microsoft.com/dotnet/desktop/winforms/compiler-messages/wfo1000>
- <https://learn.microsoft.com/ef/core/what-is-new/ef-core-10.0/breaking-changes>

The three **applicable** items were genuine breaks and were fixed as part of the
retarget commit (`P2.1`, `3c07feb`) because they were prerequisites for a green
build/tests. Everything else is recorded as examined-and-not-applicable.

## Applicable (real breaks) and how they were addressed

### 1. WFO1000 — WinForms code-serialization analyzer (error by default)

Targeting `net9.0-windows`/`net10.0-windows` turns on analyzer **WFO1000**
("Property does not configure the code serialization for its property content")
and reports it as an **error**. The build failed with 78 × WFO1000 across
`EZControls` and `DzentoPOS2.WinForms` (custom controls with public properties
that have no `[DefaultValue]` / `[DesignerSerializationVisibility]`).

Fix: `.editorconfig` scopes `dotnet_diagnostic.WFO1000.severity = none` to
`EZControls/**/*.cs` and `DzentoPOS2/**/*.cs`. Those controls are consumed
programmatically only (never opened in the Visual Studio designer), which is the
Microsoft-documented approach for this situation.

### 2. `ListControl.SelectedValue` re-annotated as non-nullable

.NET 10 re-annotates `System.Windows.Forms.ListControl.SelectedValue` as
non-nullable, so `EZComboBox.SelectedValue` (which forwards to the inner
`ComboBox`) produced `CS8601: Possible null reference assignment`. The setter
forwards `object?`, and the inner setter rejects `null`.

Fix: annotate `EZComboBox.SelectedValue` with `[DisallowNull]` and add
`using System.Diagnostics.CodeAnalysis;` in
`EZControls/Controls/Inputs/EZComboBox.cs`.

### 3. EF Core 10 — SQLite `AddForeignKey` table rebuild copies the full column list

EF Core 10 rebuilds a table for `AddForeignKey` on SQLite by creating
`ef_temp_<Table>` **with every column of the target shape** and then running:

```sql
INSERT INTO "ef_temp_CashTransactions" ("Id", "Amount", "CashSessionId", ...)
SELECT "Id", "Amount", "CashSessionId", ... FROM "CashTransactions";
```

The `E3_CashSessionLinking` migration only emits the foreign keys; the columns
`CashTransactions.CashSessionId`/`SourceId`/`SourceType` and
`SalesInvoices.CashSessionId` were never added by any migration (they were lost
when E3 was regenerated — see `LegacySchemaRepair`'s history note). EF Core 8
recreated them implicitly during the rebuild; EF Core 10 does not, so **fresh
databases** failed with `SQLite Error 1: no such column: "CashSessionId"`.

Fix: define the four columns in the always-applied baseline migration
(`20260517183403_InitialCreate`, `Up` adds / `Down` drops). Legacy databases are
untouched: `LegacySchemaRepair` already adds the same columns idempotently for
any database with a migration history, including the removed-E3 state, so the
final schema is identical and no legacy state regresses. Verified by the full
integration suite (41/41) plus the legacy-upgrade regression test.

## Applicable to this codebase, but no change required

### EF Core 10 — parameterized collections use multiple scalar parameters

`.Contains()` on a local collection now translates to `IN (@p1, @p2, ...)`
instead of a JSON-array parameter. Used in `RolePermissionSeeder`,
`PermissionSeeder` and `AppSettingSeeder`. The translation is functionally
equivalent and the collections are small, so query results are unchanged (the
integration suite seeds through this path).

### Microsoft.Data.Sqlite 10.0 — `DateTimeOffset` / timezone handling

Three high-impact provider changes (read `DateTimeOffset` without offset assumes
UTC; writing `DateTimeOffset` to a REAL column writes UTC; `GetDateTime` with an
offset returns UTC). The solution contains **no `DateTimeOffset` usage**, so
there is no impact. (`DateTime` values are stored/read as UTC text as before.)

### .NET SDK 10 — NuGet audit and reference pruning

`dotnet restore` now audits transitive packages (already covered by the Step 2
scan), and `NU1510` is raised for direct references that NuGet prunes. The build
produces no `NU1510` and every `PackageReference` has an explicit version, so
nothing to change. Note for tooling: the .NET 10 CLI writes non-command output
to `stderr` (scripts that parse stdout must account for it).

## Examined and not applicable

| Area | Change | Why not applicable |
| --- | --- | --- |
| WinForms | WPF+WinForms `MenuItem`/`ContextMenu` disambiguation | No WPF reference |
| WinForms | `TreeView` checkbox image truncation | `TreeView` is only themed in `ThemeManager`; no checkbox/`ImageList` usage |
| WinForms | `StatusStrip` defaults to System render mode | No `StatusStrip` usage |
| WinForms | `System.Drawing` throws `ExternalException` instead of `OutOfMemoryException` | No `OutOfMemoryException` handling around GDI |
| WinForms / BCL | API obsoletions | Build produces no new obsoletion warnings |
| Serialization | `System.Text.Json` property-name conflict check | `AppSettings` JSON names are unique/PascalCase |
| Serialization | `XmlSerializer` no longer ignores `ObsoleteAttribute` members | No `XmlSerializer` usage |
| Extensions | Null values preserved in configuration | `appsettings.json` has no null values |
| Extensions | `ProviderAliasAttribute` moved to Logging.Abstractions | Not referenced |
| Extensions | `BackgroundService` executes as a `Task` | Not a hosted-service app |
| Core libraries | `FilePatternMatch.Stem` non-nullable; `BufferedStream.WriteByte` flush; `ActivitySource` sampling; LINQ `AsyncEnumerable` | APIs not used |
| EF Core 10 | SQL Server broker dependency, `Application Name` injection, `json` data type | SQLite provider only |
| EF Core 10 | EF tools require a framework flag for multi-targeted projects | All projects are single-targeted |
| EF Core 10 | Complex type column naming / uniquification | No complex or owned types (model snapshot is clean) |
| EF Core 10 | SQL parameter names simplified (`@__x_0` → `@x`) | No tests/interceptors assert on generated SQL text or parameter names |

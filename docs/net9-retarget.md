# .NET 9 target — decision and verification

The solution targets **`net9.0`** (libraries) and **`net9.0-windows`** (the
WinForms app and `EZControls`), with the Microsoft packages aligned to **9.0.22**.

This document records why the target is .NET 9 and not .NET 10, and the
verification performed on .NET 9.

## Why not .NET 10

Visual Studio 2022 (17.14, MSBuild 17.14.10) cannot build a `net10.0` project:

1. The .NET 10 SDK (`10.0.403`) ships `minimumMSBuildVersion = 18.0.0`
   (`sdk\10.0.403\minimumMSBuildVersion`). MSBuild **17.14** refuses to load it.
2. MSBuild therefore falls back to the machine-wide .NET 9 SDK (`9.0.300`,
   `minimumMSBuildVersion = 17.12.0`).
3. That SDK sees a `net10.0` target while `BuildingInsideVisualStudio=true` and
   reports **`NETSDK1209`**:

   ```
   NETSDK1209: The current Visual Studio version does not support targeting
   .NET 10.0.  Either target .NET 9.0 or lower, or use Visual Studio
   version 17.16 or higher
   ```

The "Visual Studio version 17.16 or higher" text is a **stale placeholder**
(`MinimumVisualStudioVersionForUnsupportedTargetFrameworkVersion = 17.16` in the
9.0.300 SDK); no VS 2022 17.x can load the .NET 10 SDK. The real requirement is
**MSBuild 18.0 = Visual Studio 2026**.

Reproduced outside the IDE (so it is a machine/toolchain constraint, not a repo
bug):

```
> & "<VS>\MSBuild\Current\Bin\MSBuild.exe" <app>.csproj -t:Build -p:BuildingInsideVisualStudio=true

A compatible .NET SDK was not found. Requested SDK version: 10.0.403
...
C:\Program Files\dotnet\sdk\9.0.300\...\Microsoft.NET.TargetFrameworkInference.targets(171,5):
error NETSDK1209: The current Visual Studio version does not support targeting .NET 10.0. ...
```

(Note: `%USERPROFILE%\.dotnet` holds the 10.0.403 SDK, but Visual Studio only
probes the machine-wide `C:\Program Files\dotnet`, which has 9.0.300. Moving the
SDK would not help, because of the MSBuild 18 gate.)

The command-line build is unaffected — that is why the .NET 10 verification
passed — but Visual Studio (the project's primary IDE) could not load it.

## What the .NET 9 target changed

- Target frameworks: `net10.0` → **`net9.0`**, `net10.0-windows` →
  **`net9.0-windows`**.
- Microsoft packages: `10.0.14` → **`9.0.22`** (EF Core, Sqlite, Design, Tools,
  `Microsoft.Extensions.Configuration*`, `Microsoft.Extensions.DependencyInjection.Abstractions`).
- `global.json` **removed** (the pinned `10.0.403` SDK is not usable from VS 2022).
- `AppDbContextModelSnapshot.cs`: `ProductVersion` `"10.0.14"` → `"9.0.22"`.

The following fixes from the .NET 10 attempt were **kept** — they are still
required on .NET 9 / EF Core 9:

- `.editorconfig` suppression of WinForms analyzer **WFO1000** (active on
  `net9.0-windows`).
- `EZComboBox.SelectedValue` annotated `[DisallowNull]`.
- The explicit cash-session columns in the baseline `InitialCreate` migration:
  EF Core **9** also rebuilds a SQLite table for `AddForeignKey` by copying the
  full target column list, so fresh databases fail with `no such column` without
  them.
- A repository-scoped `NuGet.config` (nuget.org + cleared `packageSourceMapping`):
  a machine-level `packageSourceMapping` that maps only the pattern `"new"`
  silently downgrades unavailable packages to cached ones, which blocked
  downloading 9.0.22.

## Verification (on .NET 9, machine-wide SDK 9.0.300)

All commands run from the repository root with
`C:\Program Files\dotnet\dotnet.exe` (the SDK Visual Studio uses).

### Build

```
dotnet build DzentoPOS2.sln -t:Rebuild
```

| | Baseline (.NET 8) | .NET 10 attempt | After retarget (.NET 9) |
| --- | --- | --- | --- |
| Errors | 0 | 0 | **0** |
| Warnings | 21 | 21 | **21** |

Every warning is `NU1701` (packages restored for .NET Framework targets:
`AForge*`, `OpenTK*`, `SkiaSharp.Views.WindowsForms`). Pre-existing and
unchanged.

### Tests

```
dotnet test DzentoPOS2.sln --no-build
```

| Suite | Result |
| --- | --- |
| DzentoPOS2.UnitTests | **84 / 84 passed** |
| DzentoPOS2.IntegrationTests | **41 / 41 passed** |

### Migrations

- **Model vs snapshot:** `dotnet ef migrations has-pending-model-changes
  --project DzentoPOS2.Data` → *"No changes have been made to the model since the
  last migration."*
- **Fresh database:** `dotnet ef database update --connection "Data Source=<temp>"`
  applies all **9** migrations (`Done.`) and produces the full schema
  (**56 tables**), including the cash-session columns the E3 table rebuild
  depends on.

The EF warning *"migration 'E3_CashSessionLinking' ... cannot be executed in a
transaction"* is informational (SQLite rebuilds the table for `AddForeignKey`)
and is inherent to that migration, not a regression.

## Historical documents

The `docs/net10-*.md` files record the (now reverted) .NET 10 attempt:

- [`net10-package-audit.md`](net10-package-audit.md) — package/licence/vulnerability audit.
- [`net10-breaking-changes.md`](net10-breaking-changes.md) — .NET 10 / EF Core 10 review.
- [`net10-verification.md`](net10-verification.md) — .NET 10 build/test/migration verification.
- [`net10-smoke-run.md`](net10-smoke-run.md) — startup smoke run (framework-agnostic).

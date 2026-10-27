# .NET 10 package audit (P2 / Step 2)

> **Historical (reverted).** This documents the .NET 10 retarget, which was reverted because Visual Studio 2022 cannot build `net10.0`. The solution now targets .NET 9 — see [`net9-retarget.md`](net9-retarget.md). The package findings below (licences, vulnerabilities, deprecation) are still valid; the Microsoft package versions are now `9.0.22`.

Scope: every `PackageReference` in the solution, checked for .NET 10
compatibility, newer stable releases, license terms, vulnerabilities and
deprecation.

**Outcome: no package versions were changed in this step.** The audit confirmed
that the versions pinned during Step 1 are the latest stable line suitable for
this solution, and the one candidate removal (`System.Data.SqlClient`) turned out
to be a required runtime dependency.

## Packages (unchanged)

| Package | Version | net10 OK | Latest stable | License |
| --- | --- | --- | --- | --- |
| Microsoft.EntityFrameworkCore | 10.0.14 | yes | 10.0.14 | MIT |
| Microsoft.EntityFrameworkCore.Sqlite | 10.0.14 | yes | 10.0.14 | MIT |
| Microsoft.EntityFrameworkCore.Design | 10.0.14 | yes | 10.0.14 | MIT |
| Microsoft.EntityFrameworkCore.Tools | 10.0.14 | yes | 10.0.14 | MIT |
| Microsoft.Extensions.Configuration(.FileExtensions/.Json) | 10.0.14 | yes | 10.0.14 | MIT |
| Microsoft.Extensions.DependencyInjection.Abstractions | 10.0.14 | yes | 10.0.14 | MIT |
| Microsoft.NET.Test.Sdk | 17.12.0 | yes | 18.11.0 | MIT |
| System.Data.SqlClient | 4.8.6 | yes | 4.9.1 | MIT |
| System.Windows.Forms.DataVisualization | 1.0.0-prerelease.20110.1 | yes (netcoreapp3.0 ref) | none (2020 prerelease only) | MIT |
| AutoMapper | 16.1.1 | yes (`net10.0` target) | 16.2.0 | RPL-1.5 **or** commercial — see below |
| QuestPDF | 2024.12.3 | yes (net6.0/net8.0) | 2026.9.1 | MIT **or** paid tier — see below |
| ZXing.Net | 0.16.8 | yes (net6.0) | 0.16.11 | Apache-2.0 |
| FontAwesome.Sharp | 6.6.0 | yes (net8.0-windows) | 6.6.0 | Apache-2.0 |
| AForge.Video.DirectShow (+ AForge, AForge.Video, AForge.Math) | 2.2.5 | yes (frameworks, NU1701) | 2.2.5 | LGPL-3.0 |
| SkiaSharp / SkiaSharp.Views.WindowsForms | 3.119.2 | yes | 4.153.1 | MIT |
| Svg.Skia | 5.0.0 | yes (`net10.0` target) | 5.2.3 | MIT |
| BCrypt.Net-Next | 4.2.0 | yes (`net10.0` target) | 4.2.1 | MIT |
| FluentValidation / .DependencyInjectionExtensions | 12.1.1 | yes (net8.0) | 12.2.0 | Apache-2.0 |
| xunit | 2.9.3 | yes | v3 (2.9.3 is flagged Legacy) | Apache-2.0 |
| xunit.runner.visualstudio | 2.8.2 | yes | 4.0.1 | Apache-2.0 |

No downgrades were introduced. Newer stable releases listed above were **not**
adopted in this step (deliberate risk minimisation for a framework retarget).

## Vulnerabilities

`dotnet list DzentoPOS2.sln package --vulnerable --include-transitive` reports
**no vulnerable packages** in any project (direct or transitive).

## Deprecation

`dotnet list DzentoPOS2.sln package --deprecated` flags a single package:

- **xunit 2.9.3** — reason `Legacy`, suggested alternative `xunit.v3 >= 0.0.0`,
  in both test projects.

Migrating to xunit v3 is a behavioural change to the test host and is out of
scope for the framework retarget. Recorded here; `Microsoft.NET.Test.Sdk`
17.12.0 continues to run the net10 suites (84 + 41 tests) without issue.

## License findings that need a decision

### AutoMapper 16.1.1 — dual license (RPL-1.5 or commercial)

The shipped `LICENSE.md` states the code/binaries are governed by the
**Reciprocal Public License 1.5 (RPL-1.5)**; to use them without releasing your
own source under RPL-1.5 you must use the commercial **License Agreement**
(<https://luckypennysoftware.com/license>).

RPL-1.5 is a strong reciprocal/copyleft licence. If DzentoPOS2 is distributed
as a closed-source commercial product, **a paid commercial AutoMapper licence is
required**. This needs a business decision; nothing was changed here.

### QuestPDF 2024.12.3 — MIT or paid tier, based on revenue

`PackageLicense.md` grants the **MIT** licence free of charge to non-profits,
educational users, and companies with annual gross revenue below **US $1M**.
Above that threshold a paid **Professional** (≤ 10 developers) or **Enterprise**
(> 10 developers) licence is required. Redistribution is royalty-free.

Whether a paid QuestPDF tier is needed depends on the company's annual revenue;
a business decision, not a code change.

### AForge.Video.DirectShow 2.2.5 — LGPL-3.0

The package has no SPDX expression; its (dead) `licenseUrl` points at
aforgenet.com. The project states: *"the AForge.Video.FFMPEG component is GPL v3
licensed … The rest of the framework is LGPL v3 licensed."* DzentoPOS2 uses
`AForge.Video.DirectShow` (camera capture), which is part of the **LGPL-3.0**
portion, not FFMPEG.

LGPL-3.0 is usable in a closed-source product provided the library stays
dynamically linked, unmodified, and the licence/notice is preserved — this is a
distribution/legal review item, not a code change.

## `System.Data.SqlClient` — kept (runtime dependency, not unused)

`System.Data.SqlClient` 4.8.6 has **no compile-time references** in the source
(`SqlConnection`/`SqlCommand`/etc. do not appear), which made it a removal
candidate. It was kept because it is a genuine **runtime** dependency:

- `ChartsModule` uses `System.Windows.Forms.DataVisualization.Charting`.
- `System.Windows.Forms.DataVisualization.dll` has a hard assembly reference to
  `System.Data.SqlClient, Version=4.6.0.0` (verified from the package's
  AssemblyRef table), yet its nuspec declares **no** dependencies, so it does not
  flow transitively.
- `CHANGELOG.md` already documents the exact runtime failure caused by removing
  it (line ~407), which is why it was re-added explicitly.

Removing the reference would therefore regress the Dashboard charts at runtime.
It stays at 4.8.6 (MIT); 4.9.1 is available but was not adopted.

## `System.Windows.Forms.DataVisualization` — usage and alternatives

Only `DzentoPOS2/Modules/Dashboard/ChartsModule.cs` uses it (line/column/pie
charts). The package has **no stable release and has not been maintained since
2020** (`1.0.0-prerelease.20110.1`, targeting `netcoreapp3.0`).

Stable alternatives (both MIT), **not adopted** because the replacement is
non-trivial and would change chart visuals:

- `ScottPlot.WinForms` (5.x)
- `LiveChartsCore.SkiaSharpView.WinForms` (2.x) — SkiaSharp is already a
  dependency, so this would reuse an existing renderer.

Recommendation: schedule a separate task to migrate the Dashboard charts before
the DataVisualization package becomes a hard blocker.

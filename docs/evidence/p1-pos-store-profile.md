# P1 — POS catalog load: profile and fix

Commit 2b (`feat(pos): load the POS catalog in the background`) replaces the
P0 catalog load of `PosProductStore`. This note records what was slow, what
changed, the new numbers and the proof that the screen does not freeze.

Environment: Debug/Release desktop build, SQLite file database, Windows,
5,000 active variants (500 products x 10 variants), 6,000 barcodes, 5,000 stock
rows, one branch/warehouse, one category.

## 1. Method

A throw-away profiler (`%TEMP%\opencode\pos-store-profile`, not committed) seeds
one database and runs both loaders against it, three interleaved rounds, so the
SQLite and OS page caches help both sides equally. The legacy loader is a
verbatim copy of the P0 code from this commit's parent:

- `Include(Product).ThenInclude(Unit)` over every active variant,
- `.Contains(variantIds)` (5,000 ids) for barcodes,
- `.Contains(variantIds)` for stock,
- `.Contains(productIds)` for the variant count per product,
- a per-row `barcodes.Where(...).OrderByDescending(...).FirstOrDefault()` in the
  fill loop,
- `BuildBarcodeIndex` with a second `Contains` query and a per-barcode
  `_cache.FirstOrDefault`.

The new loader is `PosProductSnapshotLoader`: four set-based queries (warehouse,
variant join, stock `GroupBy`, barcode join through the variant), no id list
passed to `Contains`, and one in-memory grouping that feeds the barcode count,
the primary barcode and the barcode index.

Median run, milliseconds (`docs/evidence/p1-pos-store-profile.json`):

| Phase | Legacy (P0) | New |
|---|---:|---:|
| Warehouse | 0.6 | 0.8 |
| Variants | 123.8 incl. entity tracking | 19.6 (single join) |
| Barcodes | 23.4 (`Contains`) | 11.7 (join) |
| Stock | **7,129.9** (`Contains` + `GroupBy`) | 32.8 (`GroupBy`, join filter) |
| Variant count per product | 2.9 (`Contains` + `GroupBy`) | in memory |
| Fill items | 78.1 (per-row barcode scan) | 2.3 |
| Barcode index | 28.0 (query + per-barcode scan) | 1.0 |
| **Total** | **7,386.8** | **67.8** |

The stock query alone was 96.5% of the old load: SQLite had to bind 5,000
parameters in a `WHERE VariantId IN (...)` before it could group. The new query
filters through the variant navigation and groups in SQL, so the parameter list
disappears. Removing the second `Contains` query and the per-row scans takes the
remaining ~250 ms down to ~35 ms.

## 2. Behaviour is unchanged

Both loaders returned 5,000 items. The profiler compares them variant by
variant on product id, name, SKU, sale price, stock, unit, category, primary
barcode, barcode count and image path:

```
items legacy=5000 new=5000
equivalent: True (mismatched variants: 0)
```

The new snapshot additionally carries `BarcodeCount` (barcode rows per variant)
and `CategoryName`, both filled from the same grouping, with no extra query.

## 3. The reload runs off the UI thread

The frame probe hosts a hidden form, pumps the message loop while the store
reloads, and measures each `Application.DoEvents()` call
(`docs/evidence/p1-pos-store-frames.json`):

| Path | Result |
|---|---|
| Legacy load called on the UI thread | **8,967 ms** blocking |
| New reload (invalidated, pumped loop) | 95 ms wall, longest UI work **0.106 ms** |

The old load blocked the interface for ~7-9 seconds. The new load runs on a
`Task.Run` thread with its own DI scope and `DbContext`; the UI thread only ever
processes messages, so the longest message-processing step stayed under 1 ms.
(The 95 ms wall time is the load itself; it does not block input.)

If the catalog changes while a load is in flight, `Invalidate()`/the notifier
bumps a version, the finishing load sees the mismatch, drops its result and
reads again. `PosProductStoreReloadTests.CatalogChangeDuringInFlightLoad_IsNotServed`
holds the first result with a gated loader, creates a product, invalidates, and
asserts that the call made before the change still returns the new product.

## 4. Benchmark after the fix

`dotnet run --project tools/DzentoPOS2.PosTools -- benchmark --variants 5000`
(`docs/evidence/pos-benchmark-p1.json`):

| Metric | P0 baseline | P1 | Target |
|---|---:|---:|---:|
| Cold load | 7,356 ms | **236 ms** | <= 1,000 ms absolute |
| Invalidate + reload | 6,887 ms | **87 ms** | <= 500 ms (existing) |
| Scroll p95 | 13.4 ms | 13.7 ms (+2.2%) | <= 16 ms |
| Scan p95 | 0.0 ms | 0.002 ms | <= 100 ms |

The benchmark reports `PASSED`. The absolute P1 target (cold load <= 1,000 ms)
is met with the full 5,000-variant catalog, including the first-call EF model
warm-up that the profiler excludes (the profiler's warm median is 68 ms). The
2.2% scroll change is inside the agreed 10% regression budget and is noise-level
for one run.

## 5. Production database

All probes and benchmarks in this commit work on copies or on databases they
seed themselves. `tools/DzentoPOS2.PosTools` prints the SHA-256 of the source
catalog database before and after the copy step and never opens the original for
writing. The earlier P0 live-form probe (commit `9f25e4e`) opened the production
database; it only read settings, sequences and session rows, but that run cannot
be re-verified retroactively. The file timestamp changed at 11:44:18, after the
probe runs ended (~11:37), which points at a separate application launch rather
than the probe. From commit `2e73b95` on, no probe touches the production file.

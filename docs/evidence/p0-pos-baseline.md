# P0 baseline — sales screen data, empty product grid, performance

Date: 2026-11-15
Branch: `feature/pos-redesign` at `42f1145`
Database: `%LocalAppData%\DzentoPOS2\Database\DzentoPOS.db`

## 1. Catalog audit

Command output: `docs/evidence/p0-pos-db-audit.txt`

| Metric | Value |
|---|---|
| Products total | 2 |
| Products active and not deleted | 2 |
| Product variants | 2 |
| Product barcodes | 0 |
| Categories | 5 |
| Branches / active warehouses | 1 / 1 |
| Product stock rows | 2 |

Active products:

| Name | SKU | Variants | Barcodes | Price | CreatedAt |
|---|---|---|---|---|---|
| gdfgdfg | gdfg | 1 | 0 | 2500.00 | 2026-11-14 20:53:56 |
| بسيب | سيب | 1 | 0 | 5.00 | 2026-11-14 20:54:52 |

Both products have no image and no barcode. The variant-level query returns the
same two rows, so the POS has two sellable items.

## 2. Live screen probe (real services, real database)

A throw-away harness built the real `SalesForm` through `AddWinForms`, with the
production database and no code changes to the form, then read the private
fields after `Show()`:

```
[t=0 ms]    _products=2  gridRows=2
[t=355 ms]  _products=2  gridRows=2  filtered=2
grid.Visible=True  grid.Size={Width=824, Height=633}
  row: gdfgdfg | gdfg | 2500.0
  row: بسيب | سيب | 5.0
```

Screenshots: `docs/evidence/p0-live-sales-form.png`,
`docs/evidence/p0-live-sales-form-t0.png`.

## 3. Conclusion about the empty product grid

The binding is not broken and no filter drops these products:

- `ProductRepository.GetAllAsync` returns every product (no `IsActive` /
  `IsDeleted` predicate).
- `ProductService.GetAllAsync` maps all rows.
- `FilterProductsAsync` with an empty term selects `_products` unchanged.
- The live probe shows 2/2 rows immediately after the form is shown.

The captured screen with zero rows therefore had an empty catalog at capture
time (the two products were created at 20:53 and 20:54) or was taken against a
different/empty database. There is no code defect to preserve, so replacing the
`DataGridView` with the virtualized product list does not remove a working
feature. P1 still makes the state explicit: the product column gets a visible
item count, an empty-state message, and a reload action, so a zero-row screen can
never be mistaken for a layout bug again.

## 4. Performance baseline

`--pos-benchmark` did not run at `42f1145`: `ActiveBranchContext` was never
initialized for the temporary benchmark database, so the run crashed before the
first measurement. The benchmark now uses a fixed branch context pointing at the
branch it seeds; this changes diagnostics only.

Baseline with 5,000 variants (`docs/evidence/pos-benchmark-baseline.json`):

| Metric | Baseline |
|---|---|
| Seed | 109,464 ms |
| Cold load | 7,356 ms |
| Scroll p50 / p95 / p99 / max | 8.0 / 13.4 / 13.7 / 22.1 ms |
| Scan p50 / p95 | 0.0 / 0.0 ms |
| Invalidate + reload | 6,887 ms |

The report is marked FAILED by the pre-existing absolute thresholds
(`ColdLoad <= 500 ms`, `InvalidateReload <= 500 ms`). Those thresholds already
fail at P0 and are not introduced by this work. P1 acceptance therefore compares
against these numbers with a maximum regression of 10%, as agreed.

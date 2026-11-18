# P1 Sales Redesign — Evidence Report

This report documents the visual and functional verification of Commit 3
(rebuild of the sales layout/designer) on the `feature/pos-redesign` branch.

## Method

- A temporary SQLite database was seeded by the developer tool
  `tools/DzentoPOS2.PosTools` (`capture` command) — the production database
  is never opened by the probe. The seed contains 40 products across 5
  categories, including multi-barcode products, a long-named product without
  an image, an out-of-stock product, a low-stock product and a 3-barcode
  product among the first six cards; 40 products total to force scrolling.
- The real `SalesForm` was built against that database through the normal DI
  container, with a fixed branch context, deterministic DZD regional settings
  and an open cash session so the session chip reads Open and checkout is
  enabled.
- A borderless form was sized to each exact viewport (375 / 768 / 1440) and
  captured with `Form.DrawToBitmap`.
- Theme was applied through the private `ThemeManager.SetMode(mode, false)`
  overload so the developer theme file is never touched. Language was set
  through `ILanguageService.SetLanguage("en"/"ar")`.
- 12 combinations were captured (3 widths × 2 themes × 2 languages) plus one
  scrolled desktop capture.
- `PopulateCart` added three lines (a normal product, the long-named
  no-image product, two units of a low-stock product) and `SetTotals` set
  19 % VAT and a 5 % discount to exercise every totals row.

## Results

- Build: `dotnet build DzentoPOS2.sln` → 0 errors, 0 warnings (product code).
- Unit tests: 264/264 passing (was 261 before the new `PosPaymentPolicy` tests).
- Integration tests: 165/165 passing.
- All 12 screenshots show `catalog=True`, `session=True`, `items=40`.
- Subtotal `25,250.00 دج`, discount `1,262.50 دج`, hero total `28,545.13 دج`
  (Arabic-Indic digits normalised to Western as required).
- Desktop (1440): 4 card columns, side-by-side product/basket, hero total
  (36 pt bold) is the largest number.
- Tablet (768): 3 card columns, stacked at 900 px and below.
- Phone (375): 1 card column, basket stacked below products, scan button
  becomes icon-only and the category strip shows a horizontal scrollbar.

## Findings addressed in this commit

1. `EZInputBase.SetPlaceholder` early-returned when an active placeholder
   was still in `InnerTextBox.Text`, so localized placeholders never
   replaced their English predecessor. Fixed by clearing the active
   placeholder first.
2. `CategoryChipBar` had no room for its horizontal scrollbar at 375 and
   showed a vertical scrollbar instead. The product column's category row is
   now 56 px in compact mode.
3. The "Scan barcode" button (150 px) stole too much width from the search
   field at 375. It becomes icon-only (52 px) and the totals value column
   widens to 68 % at phone width so the 26 pt discount value stops clipping.
4. Complete Sale wrapped in LTR at 375 because the action columns were
   30/30/40 %. Compact mode now uses 22/22/56 %.
5. The designer was missing `_basketScroll.Controls.Add(_basketLayout)` and
   `_totalsLayout.AutoSize = true`, so the basket content was absent and
   totals rows 7-9 were clipped. Both were added.

## Known limitations (reported, out of scope for Commit 3)

- Native WinForms scrollbars (vertical cart scrollbar, category horizontal
  scrollbar and `PosProductList` scrollbar) stay light in dark theme; the
  product list also uses a native `VScrollBar`. Owner-drawing the scrollbar
  is deferred.
- On phone (375) the hero total is below the fold because the basket takes
  62 % of the height and the totals scroll with the cart; the Complete Sale
  button is pinned, so checkout stays reachable. Pinning the totals panel
  would require restructuring the basket layout and is deferred.
- `PosFormProbe` still references the removed `_productGrid`/`_filteredProducts`
  fields; it compiles and runs but prints empty diagnostics. It is a
  developer tool only and does not affect the product.
- Hold is in-memory and app-level only; persistence and the migration remain
  pending user approval.
- Cold-load absolute thresholds from P0 (≤ 500 ms) remain unmet at P0; P1
  reports relative regression ≤ +10 % instead.

## Screenshot files

- `pos-sales-phone-375x812-{light,dark}-{ltr,rtl}.png`
- `pos-sales-tablet-768x1024-{light,dark}-{ltr,rtl}.png`
- `pos-sales-desktop-1440x900-{light,dark}-{ltr,rtl}.png`
- `pos-sales-desktop-1440x900-light-ltr-scrolled.png`

Each combination carries 40 seeded items and an open cash session.

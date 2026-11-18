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
  (38 pt bold) is the largest number, discount 26 pt.
- Tablet (768): 3 card columns, stacked at 900 px and below.
- Phone (375): 1 card column, basket stacked below products, scan button
  becomes icon-only and the category strip shows a horizontal scrollbar.

## Pre-Commit-4 revision (design surface + six UI issues)

1. Hero strip, product header, three action rows and the ten totals rows now
   live entirely in `SalesForm.Designer.cs` (static tree, neutral
   `SystemColors`). The runtime only swaps colors/fonts, so the Visual Studio
   design surface can render the form without services.
2. Dock-order overlap fixed: the page is a 110 px hero row plus a fill content
   row; the hero, product header and actions no longer slide under a fill
   sibling. On phones the page hero row is 180 px with 86/58/28 px stacked
   cards so the 38 pt total is never clipped.
3. Actions are three fixed rows (Complete Sale 46, Card/Credit/Hold/Cancel 44,
   Print/Calculator 36) instead of one clipped 62 px row.
4. Totals grid is ten fixed 30 px rows (discount, discount editor, subtotal,
   after discount, tax rate, tax amount, fees, total, paid, balance) with a
   42/58 label/value split; the discount editor spans both columns.
5. Double search border removed: `ThemeManager.ApplyToControl` no longer
   forces `TextBox.BorderStyle.FixedSingle` on the inner `TextBox` of an
   `EZInputBase`, whose own rounded border is the only one now painted.
6. The product-image fallback is a neutral framed picture glyph instead of the
   product's initial, so no-image products read as "no photo" in LTR and RTL.
7. Payment method is now the `_paymentMethod` enum driven by the Card/Credit
   buttons; the combo and its helper label were removed. Credit stays on cash
   with paid = 0, Card settles the exact total.
8. A `DesignSurface` harness (`BeginLoad(typeof(SalesForm))`) loads the form and
   the full control tree in the design-time state, confirming the designer can
   lay the form out without DI or the database.
9. Phones keep one full product card visible (the catalog is reserved 300 px)
   and drop the cart header and customer caption to leave one cart line above
   the pinned actions.

## Commit A - invoices tab isolation

The POS tree (`_pageLayout`: hero, catalog, basket, totals, actions) is added
only to `_tabNewSale`; `_tabInvoices` contains only `_invoiceToolbar`,
`_invoiceGrid` and the new `_lblNoInvoices` empty state.  To remove any stale
paint reported after the redesign:

1. `_tabs.SelectedIndexChanged` now calls `OnTabSelected`, which forces
   `_pageLayout.Visible` false on the invoices tab and
   `_invoiceToolbar`/`_invoiceGrid` false on the New Sale tab, then re-runs the
   sale layout or the invoice filter.
2. `FilterInvoices` shows `_lblNoInvoices` (`AppStrings.NoInvoices`, added to
   the neutral/en/ar/fr resources) when the filtered list is empty and hides
   it otherwise.
3. `ProcessCmdKey` is tab-aware: F2 focuses the invoice search on the invoices
   tab and the product search on the POS tab; F12 never completes a sale
   outside the POS tab.
4. The screenshot probe now captures both tabs for every
   width/theme/language combination and throws when the POS page, hero, grid
   or toolbar is visible on the wrong tab, when the empty state does not match
   the row count, or when the tab strip is missing.
5. The prior invoices behaviour was compared with `42f1145`: loading,
   filtering, delete-permission gating and the grid columns are unchanged; no
   open/print action existed on the invoices grid before, and an empty state
   did not exist before either (it is new).
6. Only the two main tabs exist in this worktree (`_mobileTabs`/`_rootLayout`
   are not present); the tab strip is 44 px at 375/768/1440 and does not
   overlap the page body.

## Known limitations (reported, out of scope for Commit 3)

- Native WinForms scrollbars (vertical cart scrollbar, category horizontal
  scrollbar and `PosProductList` scrollbar) stay light in dark theme; the
  product list also uses a native `VScrollBar`. Owner-drawing the scrollbar
  is deferred.
- On phone (375) the cart lines and totals scroll while Complete Sale and the
  four payment actions stay pinned; only one cart line and no totals row are
  visible without scrolling. Restructuring the phone basket into a bottom
  sheet is deferred.
- The hold recall UI and the app/shift-close warning are not wired yet; the
  app-level `IPosHoldService` only stores the cart.
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
- `pos-invoices-{phone-375x812,tablet-768x1024,desktop-1440x900}-{light,dark}-{ltr,rtl}.png`

Each combination carries 40 seeded items and an open cash session.

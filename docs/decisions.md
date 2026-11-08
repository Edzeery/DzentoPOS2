# Decisions — DzentoPOS2 Catalog System

Recorded in Phase 0. Each decision is a small ADR: context, decision, consequences.

## ADR-01 — Variants as first-class entities (real SKU/barcode/cost/price/stock/image)

- **Context:** `ProductVariant` is `Name + AdditionalPrice` only and unused anywhere.
- **Decision:** Make `ProductVariant` carry SKU, barcodes, cost, price, min stock, image,
  attribute values. Every product gets a hidden default variant; all sale/stock/profit
  operate on `VariantId`. `ProductId` retained for compatibility/reporting.
- **Consequences:** Large migration + backfill; high-risk profit regression → tests
  compare simple-product profits before/after.

## ADR-02 — Barcode uniqueness: unique by default + opt-in duplicates + internal generation

- **Context:** `Product.Barcode` and `ProductBarcodes.Barcode` both unique; scanner matches
  only `Product.Barcode`.
- **Decision:** Move barcodes to `ProductBarcode` (linked to variant); global unique index
  by default. Setting `catalog.allow_duplicate_barcodes` permits duplicates; POS scan of a
  shared barcode shows a picker. Internal barcodes generated when none provided.
- **Consequences:** Migration backfills `Product.Barcode` into default variant's barcodes;
  drop unique index on `Product.Barcode`.

## ADR-03 — ProductKind enum

- **Values:** `Standard`, `Weighed`, `Serialized`, `Service`, `DigitalCode`.
- **Context:** Needed for grocery (weighed), phones (serialized/IMEI), digital codes.
- **Decision:** Add `ProductKind` to `Product`. Default `Standard`. Drives module behaviour.

## ADR-04 — Activity modules via AppSettings feature flags

- **Context:** Need grocery, phones/electronics, digital, general retail selectable.
- **Decision:** Boolean keys `modules.<name>.enabled` in `AppSettings` (group `Modules`),
  chosen in first-run wizard, editable later. Disabling never deletes data.
- **Consequences:** No schema changes for modules; gating at service/UI level.

## ADR-05 — Images on disk, relative paths, System.Drawing only

- **Context:** No image support today; must avoid new packages.
- **Decision:** Store files under app data folder; DB holds relative paths. Resize to
  1024px (longest side) JPEG + 256px thumbnail. Lazy load + LRU + strict Dispose.
- **Consequences:** GDI leak risk managed by Dispose discipline; cache invalidation on change.

## ADR-06 — Non-destructive SQLite migrations

- **Context:** SQLite column/index changes require table rebuild.
- **Decision:** Rebuild tables where needed, backfill `VariantId` to default variant,
  backup before migration, use `DROP INDEX IF EXISTS` / `CREATE ... IF NOT EXISTS`.
- **Consequences:** Migration tests on a real DB copy; idempotent re-runs.

## ADR-07 — Brand as string (not entity) in Phase 3

- **Context:** Brand appears in scope; full brand entity deferred.
- **Decision:** `Product.Brand` string, nullable. Promote to entity only if needed later.

## ADR-08 — Attribute model (definition/value/variant-attribute)

- **Decision:** `AttributeDefinition(ProductId, Name, SortOrder)`,
  `AttributeValue(DefinitionId, Value)`, `VariantAttribute(VariantId, DefinitionId, ValueId)`.
  Simple products have no attributes and a single default variant.

## ADR-09 — POS projection + cache

- **Decision:** POS uses a lightweight projection (`PosProductItem`) with a cache
  invalidated on product/variant change; no `GetAllAsync` full loads.

## ADR-10 — Branch strategy

- **Decision:** Merge `fix/barcode-scanner` into `feature/dashboard-filters` before
  starting catalog work (done in Phase 0). Base branch: `feature/dashboard-filters`.

## ADR-12 — Variant without price or cost (Phase 5 batch 5.0)

- **Context:** With variant-based selling, a variant may not have an explicit sale price or cost. The system must handle this gracefully.
- **Decision:**
  - **Sale price missing (amount = 0):** The variant inherits the parent product's sale price.
  - **Cost missing (amount = 0):** The invoice line is marked `IsCostEstimated = true`. Profit calculation uses 0 as cost (conservative). The line is flagged so reports can distinguish estimated-cost lines.
  - **Both missing:** The sale proceeds (no hard block), but the line is marked estimated. A future policy may block sale of zero-cost variants.
- **Consequences:** Existing products without variants get a default variant created on the fly (backward compat). Sales of variants with inherited prices work transparently. Estimated-cost lines are visible in reports via the `IsCostEstimated` flag.

## ADR-11 — Phased retirement of legacy inventory columns

- **Context:** Phase 1 moved barcodes, cost, price and stock to the variant
  (`ProductVariant` + `ProductBarcode` + `ProductStock`), but four legacy members
  survive on the product layer: `Product.Barcode`, the owned `Product.CostPrice`
  columns (`CostPriceAmount`/`CostPriceCurrency`), `ProductVariant.Name` and
  `ProductVariant.AdditionalPrice`. They cannot be dropped while anything still
  reads or writes them, because `ProductId` compatibility and the existing reports
  depend on them.
- **Decision:** Retire them in three phases, each gated by an in-app audit; never
  delete rows and never drop a column in the same release that stops writing it.
  - **Phase A — stop writing:** barcodes are written only to `ProductBarcode`;
    cost is written only to `ProductVariant.CostPrice` (the default variant);
    `Name` is derived from attribute values; `AdditionalPrice` is left untouched.
    The legacy columns stay populated for one release so a rollback is lossless.
  - **Phase B — stop reading:** barcode lookup and POS scanning read
    `ProductBarcodes`; cost/profit reads resolve through the default variant; the
    variants grid shows attributes with `Product.Name` as fallback. A guard test
    fails if production code references one of the four members again.
  - **Phase C — drop:** one migration drops the four columns after
    `--migration-audit` reports 0 non-empty `Product.Barcode`, 0 readers and a
    clean migration run; every `ProductVariants` row and `ProductId` is preserved.
    `Down()` re-adds the columns as NULL.
- **Consequences:** Reporting keeps working throughout because Phase B provides a
  variant-backed cost source before the column disappears; the duplication is
  bounded to one release per phase; `--migration-audit`
  (`docs/migration-evidence-2026-11-06.md`) is the objective gate for each phase.

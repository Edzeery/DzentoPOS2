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

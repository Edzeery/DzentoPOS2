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

## ADR-13 — Offline licence: signed key + device fingerprint + gradual restriction

- **Context:** The licensing entities (`LicensePlan`, `License`, `DeviceActivation`)
  and their repositories existed from the start, but nothing validated keys, bound
  a licence to a machine, enforced plan limits or restricted an expired install.
  The product must work fully offline in shops with no reliable internet.
- **Decision:**
  - **Key format:** `base64url(payloadJson) + "." + base64url(signature)`, where the
    signature is **RSA-SHA256 (PKCS#1)** over the exact payload bytes. The vendor
    keeps the private key; each install embeds only the public key
    (`LicensePublicKey.Pem`, compiled into the assembly). A key therefore cannot be
    forged and no licence server is required.
  - **Device binding:** the payload carries a fingerprint computed by
    `IDeviceIdentifier` (`WindowsDeviceIdentifier` hashes the registry
    `MachineGuid`; a portable fallback covers tests). A key copied to another
    machine fails validation with `license.device_mismatch`.
  - **Gradual restriction:** valid → `Active`; within `ExpiringSoonDays` → warning;
    past expiry but inside `GraceDays` → still writable (`GracePeriod`); after both
    → `Expired` and writes are blocked (`EnsureWritableAsync`). Limits block
    *creation* at the plan ceiling (`license.branch_limit` / `license.user_limit`)
    rather than corrupting existing data.
  - **Unlicensed = trial:** with no key the app runs under the `Basic` plan's
    limits. `LicenseOptions.Enforce` defaults to `false` so tests and development
    are unconstrained; the shipped `appsettings.json` sets it to `true`.
  - **Enforcement is optional per service:** `ILicenseService` is an optional
    constructor parameter of `BranchService`/`UserService`/`SaleService`, so
    existing direct-construction tests keep compiling and unit tests can bypass it.
- **Consequences:** All guard checks funnel through one service; issuing keys is a
  vendor-only operation. Revocation is
  offline/device-scoped (`DeactivateAsync`) because there is no phone-home; that is
  an accepted limitation. Full details in `docs/licensing.md`.

## ADR-14 — Licence issuer is a separate vendor-only tool; public key is embedded

- **Context:** ADR-13 put the key-generation and signing code inside the same
  application that ships to customers, and read the public key from
  `appsettings.json`. Anyone could replace `PublicKeyPem` with their own key and
  sign licences for themselves — effectively a full bypass.
- **Decision:**
  - **Split the trust boundary.** A separate, vendor-only project
    `tools/DzentoPOS2.LicenseIssuer` owns key generation and signing. It is never
    shipped or installed at a customer site. The customer product no longer
    contains `Sign` or the vendor tooling (`--license-keygen` / `--license-issue` /
    `--license-verify` were removed from the app).
  - **Shared, verify-only library.** `DzentoPOS2.Licensing` holds the payload,
    validation result and codec. The customer app references it and can only
    `Verify` / `Validate`; the library contains no key material and no signing
    entry point. Serialization/token packing are shared so signed bytes match
    exactly.
  - **Vendor-only signing library.** `DzentoPOS2.Licensing.Signing` holds
    `GenerateKeyPair` / `GenerateEncryptedKeyPair` / `Sign` / `SignEncrypted`. Only
    the issuer and the automated tests reference it — **never** the customer app —
    so a customer build physically cannot sign a licence.
  - **Public key compiled in.** The customer app embeds the vendor public key in
    code (`LicensePublicKey.Pem`) instead of reading it from configuration, closing
    the public-key-swap bypass. `LicenseOptions.PublicKeyPem` defaults to the
    embedded key and is not configuration-bound; tests inject their own options.
  - **Private key at rest is encrypted.** `--generate` writes the private key as a
    password-protected PKCS#8 PEM (AES-256-CBC, PBKDF2-SHA256). It is kept on the
    vendor machine (outside the repository) with its password; `--issue` decrypts
    it in memory only.
  - **Issuance ledger.** Each issued key is appended to `issued-licenses.csv` next
    to the private key so the vendor can trace who received which key.
- **Consequences:** The only secret is the private key file plus its password;
  losing them means rotating the pair and re-issuing every customer. Because the
  public key is now baked into the binary, key rotation requires a rebuild and a
  new release. The issuer is included in the solution for developer convenience but
  must be excluded from customer packaging.

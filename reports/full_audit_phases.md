# Full Audit - Phases to Implement Later
Date: 2026-10-23
Branch: fix/e1-e3-sales-stock-cash

## Phase 1: Cash Session Enforcement
- [x] Enforce open session in UI (SalesForm) before completing sale; show warning if missing
- [x] Block sale if session closed/belongs to different cashbox (enforced by service + UI)
- [ ] Allow opening session from SalesForm with opening balance validation (to do)

## Phase 2: Discount & Pricing Logic
- [x] Review discount application: support amount or percent (amount >100 treated as value)
- [x] Add validation for discount/tax in UI with proper rounding
- [ ] Support line-level discounts if required (to do)
- [x] Audit existing invoices for discount calculations

## Phase 3: Sales Persistence & Validation
- [x] Ensure SalesInvoice.CustomerId FK or allow walk-in (Guid.Empty)
- [x] Validate paidAmount <= total? or allow partial (current allows partial)
- [x] Add check for tracked stock before sale in UI (basic)
- [x] Prevent sale of deleted/inactive products

## Phase 4: Stock & Inventory
- [x] Review StockService concurrency handling
- [ ] Add stock alerts (to do)
- [x] Verify void restores stock correctly

## Phase 5: Cash Session Integration
- [x] Link all cash transactions to session
- [x] Session summary accuracy
- [x] Closing balance validation

## Phase 6: Migrations & Data Integrity
- [ ] Add migration tests for legacy DB
- [ ] Ensure FKs/cascades correct
- [ ] PRAGMA foreign_key_check on upgraded DB






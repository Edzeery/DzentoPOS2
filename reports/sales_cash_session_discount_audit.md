# Sales / Cash Session / Discount Audit Report
Date: 2026-10-23

## Issues observed
- SaleService.CreateAsync requires open CashSession for default cashbox and throws CashSessionRequiredException if none.
- Tests needed open session; fixture now creates open cash session with OpeningBalance 1000.
- SaleService now rejects session with OpeningBalance < 0.

## Discount/Payment behavior
- InvoiceCalculator: discount % on subtotal, tax % on (subtotal - discountAmount), rounded independently to currencyDecimalDigits (AwayFromZero).
- PaidAmount rounded; remaining = max(0,total-paid), change = max(0,paid-total).
- SalesInvoice persists PaidAmount/RemainingAmount from calculator.
- SalesForm auto-fills paid to total until user edits.

## Migrations
- E1: invoice totals/breakdown and document sequence
- E2: inventory/void
- E3: CashSessionId/SourceType/SourceId to CashTransaction and SalesInvoice, indexes/FKs

## Tests
- Unit: 21 passed
- Integration: 9 passed

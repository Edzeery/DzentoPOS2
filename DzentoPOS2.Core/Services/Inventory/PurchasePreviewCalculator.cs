using System;
using System.Collections.Generic;
using System.Linq;

namespace DzentoPOS2.Core.Services.Inventory
{
    public sealed record PurchasePreviewLineInput(
        Guid VariantId,
        decimal CurrentStockQty,
        decimal CurrentCostPrice,
        decimal PurchaseQty,
        decimal PurchaseCostPrice,
        decimal SalePrice);

    public sealed record PurchasePreviewLineResult(
        Guid VariantId,
        decimal PurchaseQty,
        decimal NewCostPrice,
        decimal SalePrice,
        decimal UnitMargin,
        decimal ExpectedLineProfit);

    public sealed record PurchasePreviewResult(
        IReadOnlyList<PurchasePreviewLineResult> Lines,
        decimal TotalPurchaseCost,
        decimal ExpectedProfit,
        decimal InventoryImpact);

    public static class PurchasePreviewCalculator
    {
        public static PurchasePreviewResult Calculate(
            IEnumerable<PurchasePreviewLineInput> lines,
            Func<Guid, decimal> discountFactor,
            decimal feeFactor)
        {
            if (lines is null) throw new ArgumentNullException(nameof(lines));
            if (discountFactor is null) throw new ArgumentNullException(nameof(discountFactor));

            var results = new List<PurchasePreviewLineResult>();
            decimal totalCost = 0m;
            decimal expectedProfit = 0m;
            decimal inventoryImpact = 0m;

            foreach (var l in lines)
            {
                if (l.PurchaseQty <= 0) continue;

                decimal newCost;
                var oldQty = l.CurrentStockQty;
                if (oldQty > 0)
                {
                    var totalQty = oldQty + l.PurchaseQty;
                    newCost = totalQty > 0
                        ? (oldQty * l.CurrentCostPrice + l.PurchaseQty * l.PurchaseCostPrice) / totalQty
                        : l.PurchaseCostPrice;
                }
                else
                {
                    newCost = l.PurchaseCostPrice;
                }

                var d = Math.Clamp(discountFactor(l.VariantId), 0m, 0.5m);
                var f = Math.Clamp(feeFactor, 0m, 0.25m);
                var nrv = l.SalePrice * (1 - d) * (1 - f);
                var unitMargin = l.SalePrice - newCost;
                var realisticUnit = nrv - newCost;
                var lineProfit = l.PurchaseQty * realisticUnit;

                results.Add(new PurchasePreviewLineResult(
                    l.VariantId, l.PurchaseQty, newCost, l.SalePrice, unitMargin, lineProfit));

                totalCost += l.PurchaseQty * l.PurchaseCostPrice;
                expectedProfit += lineProfit;

                // impact on inventory expected profit: after - before for this variant
                var beforeQty = oldQty > 0 ? oldQty : 0m;
                var beforeExpected = beforeQty > 0
                    ? beforeQty * (l.SalePrice * (1 - d) * (1 - f) - l.CurrentCostPrice)
                    : 0m;
                var afterQty = beforeQty + l.PurchaseQty;
                var afterExpected = afterQty > 0
                    ? afterQty * (l.SalePrice * (1 - d) * (1 - f) - newCost)
                    : 0m;
                inventoryImpact += afterExpected - beforeExpected;
            }

            return new PurchasePreviewResult(results, totalCost, expectedProfit, inventoryImpact);
        }
    }
}
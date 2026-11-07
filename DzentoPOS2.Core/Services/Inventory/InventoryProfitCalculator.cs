using System;
using System.Collections.Generic;
using System.Linq;

namespace DzentoPOS2.Core.Services.Inventory
{
    public sealed record InventoryProductInput(
        Guid VariantId,
        decimal SalePrice,
        decimal CostPrice,
        bool IsActive,
        bool TrackStock,
        Guid CategoryId);

    public sealed record InventoryStockInput(Guid VariantId, decimal Quantity);

    public sealed record InventoryProfitResult(
        decimal CostValue,
        decimal RetailValue,
        decimal PotentialProfit,
        decimal PotentialMarginPct,
        decimal RealisticProfit,
        decimal AtRiskProfit,
        int WarningNoSalePrice,
        int WarningNegativeMargin,
        int WarningZeroCost);

    public static class InventoryProfitCalculator
    {
        public static InventoryProfitResult Calculate(
            IEnumerable<InventoryProductInput> products,
            IEnumerable<InventoryStockInput> stocks,
            Func<Guid, decimal> discountFactor, // d per product
            decimal feeFactor) // f
        {
            if (products is null) throw new ArgumentNullException(nameof(products));
            if (stocks is null) throw new ArgumentNullException(nameof(stocks));
            if (discountFactor is null) throw new ArgumentNullException(nameof(discountFactor));

            var stockByVariant = stocks
                .GroupBy(s => s.VariantId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

            decimal costValue = 0m, retailValue = 0m, potential = 0m, realistic = 0m, atRisk = 0m;
            int noSale = 0, negMargin = 0, zeroCost = 0;

            foreach (var p in products)
            {
                if (!p.IsActive || !p.TrackStock) continue;
                if (!stockByVariant.TryGetValue(p.VariantId, out var qty)) continue;
                if (qty <= 0) continue;

                if (p.SalePrice <= 0) { noSale++; }
                if (p.CostPrice == 0) { zeroCost++; }
                var margin = p.SalePrice - p.CostPrice;
                if (margin < 0) { negMargin++; }

                costValue += qty * p.CostPrice;
                retailValue += qty * p.SalePrice;
                potential += qty * margin;

                var d = discountFactor(p.VariantId);
                d = Math.Clamp(d, 0m, 0.5m);
                var f = Math.Clamp(feeFactor, 0m, 0.25m);
                var nrv = p.SalePrice * (1 - d) * (1 - f);
                var unit = nrv - p.CostPrice; // negative margin stays negative
                realistic += qty * unit;
                if (unit < 0) atRisk += qty * unit;
            }

            var marginPct = retailValue > 0 ? potential / retailValue * 100m : 0m;

            return new InventoryProfitResult(
                costValue, retailValue, potential, marginPct, realistic, atRisk,
                noSale, negMargin, zeroCost);
        }
    }
}
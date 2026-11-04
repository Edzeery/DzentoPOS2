using System;
using System.Collections.Generic;
using System.Linq;

namespace DzentoPOS2.Core.Services.Sales
{
    /// <summary>
    /// Single source of truth for profit math (see task spec).
    /// All inputs are already-frozen line values; changing a product's cost later
    /// does NOT change a previously computed result because the caller passes the
    /// frozen <see cref="ProfitLine.UnitCost"/> / <see cref="ProfitLine.ListPrice"/>.
    /// </summary>
    public sealed record ProfitLine(
        decimal Quantity,
        decimal Price,
        decimal ListPrice,
        decimal UnitCost,
        decimal DiscountShare = 0m);

    public sealed record InvoiceProfit(
        decimal Revenue,
        decimal Cost,
        decimal ExpectedProfit,
        decimal GrossProfit,
        decimal Fees,
        decimal ActualProfit,
        decimal OperatingExpenses,
        decimal NetProfit,
        decimal ReturnsNetEffect,
        decimal GrossMargin,
        decimal NetMargin,
        IReadOnlyList<decimal> LineNets,
        IReadOnlyList<decimal> LineCosts,
        IReadOnlyList<decimal> LineExpecteds);

    public static class ProfitCalculator
    {
        public static decimal Round(decimal value, int digits) =>
            decimal.Round(value, digits, MidpointRounding.AwayFromZero);

        /// <summary>
        /// Distributes an invoice-level discount across lines proportionally to
        /// each line's gross (Price × Qty). The rounding remainder is added to
        /// the last line.
        /// </summary>
        public static IReadOnlyList<decimal> DistributeDiscount(
            IReadOnlyList<decimal> lineGrosses,
            decimal invoiceDiscountAmount,
            int digits = 2)
        {
            if (lineGrosses is null)
                throw new ArgumentNullException(nameof(lineGrosses));

            var count = lineGrosses.Count;
            var result = new decimal[count];
            if (count == 0)
                return result;

            var grossSum = lineGrosses.Sum();
            var allocated = 0m;
            for (int i = 0; i < count; i++)
            {
                if (i == count - 1)
                {
                    result[i] = Round(invoiceDiscountAmount - allocated, digits);
                }
                else
                {
                    var share = grossSum > 0
                        ? Round(invoiceDiscountAmount * lineGrosses[i] / grossSum, digits)
                        : 0m;
                    result[i] = share;
                    allocated += share;
                }
            }
            return result;
        }

        public static decimal LineGross(decimal price, decimal quantity) =>
            price * quantity;

        public static decimal LineNet(decimal price, decimal quantity, decimal discountShare, int digits = 2) =>
            Round(price * quantity - discountShare, digits);

        public static decimal LineCost(decimal unitCost, decimal quantity, int digits = 2) =>
            Round(unitCost * quantity, digits);

        public static decimal LineExpected(decimal listPrice, decimal unitCost, decimal quantity, int digits = 2) =>
            Round((listPrice - unitCost) * quantity, digits);

        public static decimal CalculateExpectedProfit(IEnumerable<ProfitLine> lines, int digits = 2)
        {
            if (lines is null) throw new ArgumentNullException(nameof(lines));
            return Round(lines.Sum(l => (l.ListPrice - l.UnitCost) * l.Quantity), digits);
        }

        public static decimal CalculateExpectedStockProfit(
            IEnumerable<(decimal Qty, decimal SalePrice, decimal Cost)> items,
            int digits = 2)
        {
            if (items is null) throw new ArgumentNullException(nameof(items));
            decimal total = 0m;
            foreach (var (qty, salePrice, cost) in items)
            {
                if (qty <= 0) continue;
                total += qty * (salePrice - cost);
            }
            return Round(total, digits);
        }

        public static decimal CalculateGrossProfit(decimal revenue, decimal cost, decimal returnsNetEffect = 0m) =>
            revenue - cost - returnsNetEffect;

        public static decimal CalculateActualProfit(decimal grossProfit, decimal fees) =>
            grossProfit - fees;

        public static decimal CalculateNetProfit(decimal actualProfit, decimal operatingExpenses) =>
            actualProfit - operatingExpenses;

        public static decimal CalculateMargin(decimal profit, decimal revenue, int digits = 2) =>
            revenue > 0 ? Round(profit / revenue * 100m, digits) : 0m;

        /// <summary>
        /// Full invoice profit breakdown. Fees and operating expenses are passed
        /// in (they are not part of the line math).
        /// </summary>
        public static InvoiceProfit CalculateInvoice(
            IReadOnlyCollection<ProfitLine> lines,
            decimal fees = 0m,
            decimal operatingExpenses = 0m,
            decimal returnsNetEffect = 0m,
            int digits = 2)
        {
            if (lines is null) throw new ArgumentNullException(nameof(lines));

            var lineNets = new List<decimal>();
            var lineCosts = new List<decimal>();
            var lineExpecteds = new List<decimal>();

            decimal revenue = 0m, cost = 0m, expected = 0m;
            foreach (var l in lines)
            {
                var ln = LineNet(l.Price, l.Quantity, l.DiscountShare, digits);
                var lc = LineCost(l.UnitCost, l.Quantity, digits);
                // Expected profit = (registered sale price - frozen cost) x qty,
                // before discounts, returns and fees.
                var le = Round((l.Price - l.UnitCost) * l.Quantity, digits);
                lineNets.Add(ln);
                lineCosts.Add(lc);
                lineExpecteds.Add(le);
                revenue += ln;
                cost += lc;
                expected += le;
            }

            revenue = Round(revenue, digits);
            cost = Round(cost, digits);
            expected = Round(expected, digits);

            var grossProfit = Round(CalculateGrossProfit(revenue, cost, returnsNetEffect), digits);
            var actualProfit = Round(CalculateActualProfit(grossProfit, fees), digits);
            var netProfit = Round(CalculateNetProfit(actualProfit, operatingExpenses), digits);

            return new InvoiceProfit(
                revenue,
                cost,
                expected,
                grossProfit,
                Round(fees, digits),
                actualProfit,
                Round(operatingExpenses, digits),
                netProfit,
                Round(returnsNetEffect, digits),
                CalculateMargin(grossProfit, revenue, digits),
                CalculateMargin(netProfit, revenue, digits),
                lineNets,
                lineCosts,
                lineExpecteds);
        }

        /// <summary>
        /// Period-level aggregation. Each tuple is one invoice's already-computed
        /// (revenue, cost, expected, fees). ReturnsNetEffect is subtracted from
        /// gross profit.
        /// </summary>
        public static InvoiceProfit CalculatePeriod(
            IReadOnlyCollection<(decimal Revenue, decimal Cost, decimal Expected, decimal Fees)> invoices,
            decimal operatingExpenses = 0m,
            decimal returnsNetEffect = 0m,
            int digits = 2)
        {
            if (invoices is null) throw new ArgumentNullException(nameof(invoices));

            decimal revenue = 0m, cost = 0m, expected = 0m, fees = 0m;
            foreach (var inv in invoices)
            {
                revenue += inv.Revenue;
                cost += inv.Cost;
                expected += inv.Expected;
                fees += inv.Fees;
            }

            revenue = Round(revenue, digits);
            cost = Round(cost, digits);
            expected = Round(expected, digits);
            fees = Round(fees, digits);

            var grossProfit = Round(CalculateGrossProfit(revenue, cost, returnsNetEffect), digits);
            var actualProfit = Round(CalculateActualProfit(grossProfit, fees), digits);
            var netProfit = Round(CalculateNetProfit(actualProfit, operatingExpenses), digits);

            return new InvoiceProfit(
                revenue,
                cost,
                expected,
                grossProfit,
                fees,
                actualProfit,
                Round(operatingExpenses, digits),
                netProfit,
                Round(returnsNetEffect, digits),
                CalculateMargin(grossProfit, revenue, digits),
                CalculateMargin(netProfit, revenue, digits),
                Array.Empty<decimal>(),
                Array.Empty<decimal>(),
                Array.Empty<decimal>());
        }
    }
}

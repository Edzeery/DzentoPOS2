using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.App.Services.Reports.Interfaces;
using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.Core.Services.Sales;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Shared.Contracts.DTOs.Reports;
using DzentoPOS2.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace DzentoPOS2.App.Services.Reports
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;
        private readonly RegionalContext _regional;
        private readonly IBusinessClock _clock;
        private readonly IInventoryValuationService _valuation;
        private readonly IInventorySnapshotService _snapshots;

        public DashboardService(
            AppDbContext context,
            RegionalContext regional,
            IBusinessClock clock,
            IInventoryValuationService valuation,
            IInventorySnapshotService snapshots)
        {
            _context = context;
            _regional = regional;
            _clock = clock;
            _valuation = valuation;
            _snapshots = snapshots;
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync(DashboardFilterDto? filter = null)
        {
            filter ??= new DashboardFilterDto { Preset = DashboardDatePreset.Today };

            var localNow = _clock.Now;
            var resolved = DashboardRangeResolver.Resolve(_clock, filter);
            var rangeStartUtc = resolved.StartUtc;
            var rangeEndUtc = resolved.EndUtc;
            var compareStartUtc = resolved.PreviousStartUtc;
            var compareEndUtc = resolved.PreviousEndUtc;
            var chartStartUtc = (filter.Preset == DashboardDatePreset.ThisMonth)
                ? _regional.StartOfLocalDayUtc(localNow.Date.AddDays(-29))
                : rangeStartUtc;

            var monthStart = rangeStartUtc;
            var nextMonthStart = rangeEndUtc;
            var todayStart = rangeStartUtc;
            var tomorrowStart = rangeEndUtc;
            var yesterdayStart = compareStartUtc;
            var lastMonthStart = compareStartUtc;
            var thirtyDaysAgo = chartStartUtc;

            var dto = new DashboardSummaryDto();

            await ComputeKpiCards(dto, todayStart, tomorrowStart, yesterdayStart, monthStart, nextMonthStart, lastMonthStart, filter?.UserId);
            await ComputeChartData(dto, thirtyDaysAgo, tomorrowStart, localNow, filter?.UserId);
            await ComputeRecentTransactions(dto);
            await ComputeInsights(dto);

            return dto;
        }

        private async Task ComputeKpiCards(DashboardSummaryDto dto,
            DateTime todayStart, DateTime tomorrowStart,
            DateTime yesterdayStart,
            DateTime monthStart, DateTime nextMonthStart, DateTime lastMonthStart,
            Guid? customerId)
        {
            // Sales — invoice value (Total), periods expressed as half-open ranges.
            // Note: SQLite cannot SUM decimal columns directly, so amounts are cast to
            // double in SQL (translated to CAST(x AS REAL)) and converted back on read.
            // One grouped query computes every sales KPI instead of six round trips.
            var salesStats = await _context.SalesInvoices
                .Where(s => !s.IsVoided && (customerId == null || s.CustomerId == customerId.Value))
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    TodayTotal = g.Sum(s => s.CreatedAt >= todayStart && s.CreatedAt < tomorrowStart ? (double?)s.Total.Amount : null) ?? 0,
                    TodayCount = g.Count(s => s.CreatedAt >= todayStart && s.CreatedAt < tomorrowStart),
                    YesterdayTotal = g.Sum(s => s.CreatedAt >= yesterdayStart && s.CreatedAt < todayStart ? (double?)s.Total.Amount : null) ?? 0,
                    YesterdayCount = g.Count(s => s.CreatedAt >= yesterdayStart && s.CreatedAt < todayStart),
                    MonthTotal = g.Sum(s => s.CreatedAt >= monthStart && s.CreatedAt < nextMonthStart ? (double?)s.Total.Amount : null) ?? 0,
                    LastMonthTotal = g.Sum(s => s.CreatedAt >= lastMonthStart && s.CreatedAt < monthStart ? (double?)s.Total.Amount : null) ?? 0
                })
                .FirstOrDefaultAsync();

            SetCardData(dto.SalesToday, (decimal)(salesStats?.TodayTotal ?? 0), (decimal)(salesStats?.YesterdayTotal ?? 0), KpiValueKind.Money);
            SetCardData(dto.SalesCount, salesStats?.TodayCount ?? 0, salesStats?.YesterdayCount ?? 0, KpiValueKind.Count);

            // Purchases — invoice value (Total); same single grouped query approach.
            var purchaseStats = await _context.PurchaseInvoices
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    TodayTotal = g.Sum(p => p.CreatedAt >= todayStart && p.CreatedAt < tomorrowStart ? (double?)p.Total : null) ?? 0,
                    TodayCount = g.Count(p => p.CreatedAt >= todayStart && p.CreatedAt < tomorrowStart),
                    YesterdayTotal = g.Sum(p => p.CreatedAt >= yesterdayStart && p.CreatedAt < todayStart ? (double?)p.Total : null) ?? 0,
                    YesterdayCount = g.Count(p => p.CreatedAt >= yesterdayStart && p.CreatedAt < todayStart),
                    MonthTotal = g.Sum(p => p.CreatedAt >= monthStart && p.CreatedAt < nextMonthStart ? (double?)p.Total : null) ?? 0,
                    LastMonthTotal = g.Sum(p => p.CreatedAt >= lastMonthStart && p.CreatedAt < monthStart ? (double?)p.Total : null) ?? 0
                })
                .FirstOrDefaultAsync();

            SetCardData(dto.PurchasesToday, (decimal)(purchaseStats?.TodayTotal ?? 0), (decimal)(purchaseStats?.YesterdayTotal ?? 0), KpiValueKind.Money);
            SetCardData(dto.PurchasesCount, purchaseStats?.TodayCount ?? 0, purchaseStats?.YesterdayCount ?? 0, KpiValueKind.Count);
            SetCardData(dto.MonthlySales, (decimal)(salesStats?.MonthTotal ?? 0), (decimal)(salesStats?.LastMonthTotal ?? 0), KpiValueKind.Money);
            SetCardData(dto.MonthlyPurchases, (decimal)(purchaseStats?.MonthTotal ?? 0), (decimal)(purchaseStats?.LastMonthTotal ?? 0), KpiValueKind.Money);

            var monthlyExpenses = await _context.Expenses
                .Where(e => e.ExpenseDate >= monthStart && e.ExpenseDate < nextMonthStart)
                .SumAsync(e => (double?)e.Amount) is double me ? (decimal)me : 0m;

            // Store operating expenses are never attributed to a single customer
            // when the dashboard is filtered by customer (accrual, period scope).
            if (customerId.HasValue)
                monthlyExpenses = 0m;

            var monthlyReturnsTotal = await _context.SalesReturns
                .Where(r => r.CreatedAt >= monthStart && r.CreatedAt < nextMonthStart
                    && (customerId == null || r.CustomerId == customerId.Value))
                .SumAsync(r => (double?)r.TotalAmount) is double mrt ? (decimal)mrt : 0m;

            // Single grouped query for month invoices with items + fees (no N+1).
            var monthRows = await (
                from item in _context.SalesInvoiceItems
                join invoice in _context.SalesInvoices on item.SalesInvoiceId equals invoice.Id
                where invoice.CreatedAt >= monthStart && invoice.CreatedAt < nextMonthStart
                    && !invoice.IsVoided
                    && (customerId == null || invoice.CustomerId == customerId.Value)
                select new
                {
                    item.SalesInvoiceId,
                    item.ProductId,
                    item.Quantity,
                    item.Price,
                    item.UnitCost,
                    item.ListPrice,
                    item.DiscountAmount,
                    item.IsCostEstimated,
                    Fees = invoice.FeesAmount.Amount
                }
            ).ToListAsync();

            // Fees come from the distinct invoice query (not repeated per line).
            var invoiceFees = await _context.SalesInvoices
                .AsNoTracking()
                .Where(i => i.CreatedAt >= monthStart && i.CreatedAt < nextMonthStart && !i.IsVoided)
                .ToDictionaryAsync(i => i.Id, i => i.FeesAmount.Amount);

            var hasEstimated = monthRows.Any(x => x.IsCostEstimated);

            // Per-invoice profit.
            decimal revenue = 0m, cost = 0m, expected = 0m, fees = 0m, monthlyDiscounts = 0m;

            // Inventory valuation (variant-level, NRV-based) replaces the old
            // product-level stock profit calculation.
            var valuation = await _valuation.GetValuationAsync();
            var expectedStock = valuation.ExpectedProfit;
            dto.InventoryCostValue = valuation.CostValue;
            dto.InventoryRetailValue = valuation.RetailValue;
            dto.InventoryPotentialProfit = valuation.PotentialProfit;
            dto.InventoryExpectedProfit = valuation.ExpectedProfit;
            dto.InventoryAtRiskProfit = valuation.AtRiskProfit;

            // Daily snapshot (upsert, once per local day).
            await _snapshots.UpsertTodayAsync(valuation);
            foreach (var g in monthRows.GroupBy(x => x.SalesInvoiceId))
            {
                var profitLines = g.Select(x => new ProfitLine(
                    x.Quantity,
                    x.Price,
                    x.ListPrice,
                    x.UnitCost,
                    x.DiscountAmount)).ToList();
                var invProfit = ProfitCalculator.CalculateInvoice(profitLines, digits: _regional.CurrencyDecimalDigits);
                revenue += invProfit.Revenue;
                cost += invProfit.Cost;
                expected += invProfit.ExpectedProfit;
                fees += invoiceFees.TryGetValue(g.Key, out var f) ? f : 0m;
                monthlyDiscounts += g.Sum(x => x.DiscountAmount);
            }

            // Returns for the period, excluding voided invoices.
            var returnRows = await (
                from r in _context.SalesReturns
                join i in _context.SalesReturnItems on r.Id equals i.SalesReturnId
                join inv in _context.SalesInvoices on r.SalesInvoiceId equals inv.Id
                where r.CreatedAt >= monthStart && r.CreatedAt < nextMonthStart
                    && !inv.IsVoided
                    && (customerId == null || r.CustomerId == customerId.Value)
                select new
                {
                    r.SalesInvoiceId,
                    i.ProductId,
                    i.Quantity,
                    i.Total,
                    i.Price
                }
            ).ToListAsync();

            // Original (frozen) line data for invoices actually referenced by returns.
            var returnInvoiceIds = returnRows.Select(r => r.SalesInvoiceId).Distinct().ToList();
            var originalRows = returnInvoiceIds.Count == 0
                ? new List<(Guid InvoiceId, Guid ProductId, decimal Qty, decimal UnitCost, decimal DiscountAmount, bool IsEstimated)>()
                : (await (
                    from item in _context.SalesInvoiceItems
                    join inv in _context.SalesInvoices on item.SalesInvoiceId equals inv.Id
                    where returnInvoiceIds.Contains(item.SalesInvoiceId)
                    select new
                    {
                        item.SalesInvoiceId,
                        item.ProductId,
                        item.Quantity,
                        item.UnitCost,
                        item.DiscountAmount,
                        item.IsCostEstimated
                    }
                ).ToListAsync())
                .GroupBy(x => new { x.SalesInvoiceId, x.ProductId })
                .Select(g => (
                    InvoiceId: g.Key.SalesInvoiceId,
                    ProductId: g.Key.ProductId,
                    Qty: g.Sum(x => x.Quantity),
                    UnitCost: g.Sum(x => x.Quantity * x.UnitCost) / (g.Sum(x => x.Quantity) == 0 ? 1 : g.Sum(x => x.Quantity)),
                    DiscountAmount: g.Sum(x => x.DiscountAmount),
                    IsEstimated: g.Any(x => x.IsCostEstimated)
                ))
                .ToList();

            var originalLookup = originalRows
                .ToDictionary(x => (x.InvoiceId, x.ProductId), x => x);

            // Fallback product costs only for products missing in originalLookup.
            var missingProductIds = returnRows
                .Select(r => r.ProductId)
                .Distinct()
                .Where(pid => !originalLookup.Keys.Any(k => k.Item2 == pid))
                .ToList();
            var productCosts = missingProductIds.Count == 0
                ? new Dictionary<Guid, decimal>()
                : await _context.Products
                    .AsNoTracking()
                    .Where(p => missingProductIds.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id, p => p.CostPrice.Amount);

            decimal returnsNetEffect = 0m;
            foreach (var rr in returnRows)
            {
                if (originalLookup.TryGetValue((rr.SalesInvoiceId, rr.ProductId), out var orig))
                {
                    if (orig.IsEstimated) hasEstimated = true;
                    decimal discountShare = 0m;
                    if (orig.Qty > 0)
                        discountShare = orig.DiscountAmount * rr.Quantity / orig.Qty;
                    var returnLineNet = rr.Total - discountShare;
                    returnsNetEffect += returnLineNet - rr.Quantity * orig.UnitCost;
                }
                else
                {
                    // Fallback to product cost and mark as estimated.
                    hasEstimated = true;
                    var uc = productCosts.TryGetValue(rr.ProductId, out var pc) ? pc : 0m;
                    returnsNetEffect += rr.Total - rr.Quantity * uc;
                }
            }

            var digits = _regional.CurrencyDecimalDigits;
            revenue = decimal.Round(revenue, digits, MidpointRounding.AwayFromZero);
            cost = decimal.Round(cost, digits, MidpointRounding.AwayFromZero);
            expected = decimal.Round(expected, digits, MidpointRounding.AwayFromZero);
            fees = decimal.Round(fees, digits, MidpointRounding.AwayFromZero);
            monthlyDiscounts = decimal.Round(monthlyDiscounts, digits, MidpointRounding.AwayFromZero);
            returnsNetEffect = decimal.Round(returnsNetEffect, digits, MidpointRounding.AwayFromZero);

            var grossProfit = ProfitCalculator.CalculateGrossProfit(revenue, cost, returnsNetEffect);
            var actualProfit = ProfitCalculator.CalculateActualProfit(grossProfit, fees);
            var netProfit = ProfitCalculator.CalculateNetProfit(actualProfit, monthlyExpenses);

            var expectedMargin = ProfitCalculator.CalculateMargin(expected, revenue, 1);
            var actualMargin = ProfitCalculator.CalculateMargin(actualProfit, revenue, 1);
            var netMargin = ProfitCalculator.CalculateMargin(netProfit, revenue, 1);

            SetCardData(dto.ExpectedProfit, expected + valuation.ExpectedProfit, 0, KpiValueKind.Money);
            SetCardData(dto.ExpectedSoldProfit, expected, 0, KpiValueKind.Money);
            SetCardData(dto.ExpectedStockProfit, expectedStock, 0, KpiValueKind.Money);
            SetCardData(dto.ActualProfit, actualProfit, 0, KpiValueKind.Money);
            SetCardData(dto.NetProfit, netProfit, 0, KpiValueKind.Money);

            dto.ExpectedProfit.TrendKind = revenue > 0 ? KpiTrendKind.Margin : KpiTrendKind.None;
            dto.ExpectedProfit.TrendPercent = expectedMargin;
            dto.ActualProfit.TrendKind = revenue > 0 ? KpiTrendKind.Margin : KpiTrendKind.None;
            dto.ActualProfit.TrendPercent = actualMargin;
            dto.NetProfit.TrendKind = revenue > 0 ? KpiTrendKind.Margin : KpiTrendKind.None;
            dto.NetProfit.TrendPercent = netMargin;

            dto.ExpectedProfit.HasEstimatedCost = hasEstimated;
            dto.ActualProfit.HasEstimatedCost = hasEstimated;
            dto.NetProfit.HasEstimatedCost = hasEstimated;

            dto.MonthlyDiscountAmount = monthlyDiscounts;
            dto.MonthlyFeesAmount = fees;
            dto.MonthlyRevenue = revenue;
            dto.MonthlyCost = cost;
            dto.MonthlyReturns = monthlyReturnsTotal;
            dto.MonthlyGrossProfit = grossProfit;
            dto.MonthlyExpenses = monthlyExpenses;

            var productStats = await _context.Products
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Active = g.Count(p => p.IsActive),
                    LastMonth = g.Count(p => p.IsActive && p.CreatedAt < monthStart)
                })
                .FirstOrDefaultAsync();

            SetCardData(dto.ActiveProducts, productStats?.Active ?? 0, productStats?.LastMonth ?? 0, KpiValueKind.Count);

            // Low stock — products whose total warehouse stock is at/below reorder level.
            var lowStockCount = await _context.Products
                .Where(p => p.IsActive && p.TrackStock)
                .Where(p => _context.ProductStocks
                    .Where(s => s.ProductId == p.Id)
                    .Sum(s => (double?)s.Quantity.Value) <= (double)p.MinimumStock.Value)
                .CountAsync();

            SetCardData(dto.LowStock, lowStockCount, 0, KpiValueKind.Count);

            var customerStats = await _context.Customers
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Active = g.Count(c => c.IsActive),
                    LastMonth = g.Count(c => c.IsActive && c.CreatedAt < monthStart)
                })
                .FirstOrDefaultAsync();

            SetCardData(dto.ActiveCustomers, customerStats?.Active ?? 0, customerStats?.LastMonth ?? 0, KpiValueKind.Count);

            var supplierStats = await _context.Suppliers
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Active = g.Count(s => s.IsActive),
                    LastMonth = g.Count(s => s.IsActive && s.CreatedAt < monthStart)
                })
                .FirstOrDefaultAsync();

            SetCardData(dto.ActiveSuppliers, supplierStats?.Active ?? 0, supplierStats?.LastMonth ?? 0, KpiValueKind.Count);
        }

        private static string MonthKey(DateTime local) => $"{local.Year}-{local.Month:D2}";

        private static void SetCardData(DashboardCardData card, decimal current, decimal previous, KpiValueKind kind)
        {
            card.Value = current;
            card.Kind = kind;

            // A trend chip is only shown when there is a comparable previous
            // period: never a fake "+0%".
            if (previous != 0)
            {
                card.TrendKind = KpiTrendKind.Change;
                card.TrendPercent = Math.Round((current - previous) / previous * 100, 1);
            }
            else
            {
                card.TrendKind = KpiTrendKind.None;
                card.TrendPercent = 0;
            }
        }

        private async Task ComputeChartData(DashboardSummaryDto dto, DateTime thirtyDaysAgoUtc, DateTime tomorrowStartUtc, DateTime localNow, Guid? customerId)
        {
            var sixMonthsAgo = _regional.ToUtcSafe(new DateTime(localNow.Year, localNow.Month, 1).AddMonths(-5));

            // A single read over the last six months feeds both the daily trend and
            // the month-over-month comparison.
            var salesRows = await _context.SalesInvoices
                .Where(s => !s.IsVoided && s.CreatedAt >= sixMonthsAgo && s.CreatedAt < tomorrowStartUtc
                    && (customerId == null || s.CustomerId == customerId.Value))
                .Select(s => new { s.CreatedAt, Total = (double?)s.Total.Amount })
                .ToListAsync();

            var firstDay = _regional.ToLocal(thirtyDaysAgoUtc).Date;

            var grouped = salesRows
                .Select(x => new { Local = _regional.ToLocal(x.CreatedAt), x.Total })
                .Where(x => x.Local.Date >= firstDay)
                .GroupBy(x => x.Local.Date)
                .ToDictionary(g => g.Key, g => (decimal)g.Sum(x => x.Total ?? 0));

            for (int i = 29; i >= 0; i--)
            {
                var date = firstDay.AddDays(i);
                dto.SalesTrend.Add(new LineChartPoint
                {
                    Label = date.ToString("MMM dd"),
                    Value = grouped.GetValueOrDefault(date, 0)
                });
            }

            var salesByMonth = salesRows
                .GroupBy(x => MonthKey(_regional.ToLocal(x.CreatedAt)))
                .ToDictionary(g => g.Key, g => (decimal)g.Sum(x => x.Total ?? 0));

            var purchasesByMonth = (await _context.PurchaseInvoices
                .Where(p => p.CreatedAt >= sixMonthsAgo && p.CreatedAt < tomorrowStartUtc)
                .Select(p => new { p.CreatedAt, Total = (double?)p.Total })
                .ToListAsync())
                .GroupBy(x => MonthKey(_regional.ToLocal(x.CreatedAt)))
                .ToDictionary(g => g.Key, g => (decimal)g.Sum(x => x.Total ?? 0));

            var allMonths = new HashSet<string>();
            foreach (var k in salesByMonth.Keys) allMonths.Add(k);
            foreach (var k in purchasesByMonth.Keys) allMonths.Add(k);

            var culture = CultureInfo.CurrentCulture;
            foreach (var key in allMonths.OrderBy(x => x))
            {
                var parts = key.Split('-');
                var dt = new DateTime(int.Parse(parts[0]), int.Parse(parts[1]), 1, 0, 0, 0, DateTimeKind.Utc);
                dto.MonthlyComparison.Add(new ComparisonPoint
                {
                    Label = dt.ToString("MMM", culture),
                    Sales = salesByMonth.GetValueOrDefault(key, 0),
                    Purchases = purchasesByMonth.GetValueOrDefault(key, 0)
                });
            }

            var categoryTotals = await (
                from item in _context.SalesInvoiceItems
                join invoice in _context.SalesInvoices on item.SalesInvoiceId equals invoice.Id
                join product in _context.Products on item.ProductId equals product.Id
                where product.CategoryId != Guid.Empty
                    && !invoice.IsVoided
                group (double?)item.Total by new { product.CategoryId, product.Category.Name } into g
                orderby g.Sum() descending
                select new { Category = g.Key.Name, Total = g.Sum() ?? 0 }
            ).Take(5).ToListAsync();

            foreach (var c in categoryTotals)
            {
                dto.CategoryDistribution.Add(new PieSliceData
                {
                    Category = string.IsNullOrWhiteSpace(c.Category) ? AppStrings.Other : c.Category,
                    Value = (decimal)c.Total
                });
            }
        }

        private async Task ComputeRecentTransactions(DashboardSummaryDto dto)
        {
            var salesRaw = await _context.SalesInvoices
                .Where(s => !s.IsVoided)
                .OrderByDescending(s => s.CreatedAt)
                .Take(5)
                .Select(s => new
                {
                    s.Id,
                    Number = s.InvoiceNumber.Value,
                    s.CustomerId,
                    Amount = s.Total.Amount,
                    s.CreatedAt,
                    PaidAmount = s.PaidAmount.Amount,
                    Total = s.Total.Amount
                })
                .ToListAsync();

            var purchaseRaw = await _context.PurchaseInvoices
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .Select(p => new
                {
                    p.Id,
                    p.InvoiceNumber,
                    p.SupplierId,
                    Amount = p.Total,
                    p.CreatedAt,
                    Paid = p.PaidAmount,
                    Total = p.Total
                })
                .ToListAsync();

            var customerIds = salesRaw.Select(s => s.CustomerId).Distinct().ToList();
            var customerNames = await _context.Customers.AsNoTracking()
                .Where(c => customerIds.Contains(c.Id))
                .Select(c => new { c.Id, c.FullName })
                .ToDictionaryAsync(x => x.Id, x => x.FullName);

            var supplierIds = purchaseRaw.Select(p => p.SupplierId).Distinct().ToList();
            var supplierNames = await _context.Suppliers.AsNoTracking()
                .Where(s => supplierIds.Contains(s.Id))
                .Select(s => new { s.Id, s.Name })
                .ToDictionaryAsync(x => x.Id, x => x.Name);

            var sales = salesRaw.Select(s => new TransactionRow
            {
                Id = s.Id,
                Type = AppStrings.TransactionSale,
                Number = s.Number,
                Party = ResolveParty(s.CustomerId, customerNames, AppStrings.WalkInCustomer),
                Amount = s.Amount,
                Date = s.CreatedAt,
                Status = s.PaidAmount >= s.Total ? AppStrings.Paid : AppStrings.Pending
            });

            var purchases = purchaseRaw.Select(p => new TransactionRow
            {
                Id = p.Id,
                Type = AppStrings.TransactionPurchase,
                Number = p.InvoiceNumber,
                Party = ResolveParty(p.SupplierId, supplierNames, p.SupplierId.ToString()),
                Amount = p.Amount,
                Date = p.CreatedAt,
                Status = p.Paid >= p.Total ? AppStrings.Paid : AppStrings.Pending
            });

            dto.RecentTransactions = sales
                .Concat(purchases)
                .OrderByDescending(t => t.Date)
                .Take(8)
                .ToList();
        }

        private static string ResolveParty(Guid id, Dictionary<Guid, string> names, string fallback)
        {
            if (id == Guid.Empty)
                return fallback;

            return names.TryGetValue(id, out var name) && !string.IsNullOrWhiteSpace(name)
                ? name
                : fallback;
        }

        private async Task ComputeInsights(DashboardSummaryDto dto)
        {
            var culture = CultureInfo.CurrentCulture;
            var digits = _regional.CurrencyDecimalDigits;

            var topProducts = (await (
                from item in _context.SalesInvoiceItems
                join product in _context.Products on item.ProductId equals product.Id
                group (double?)item.Total by new { product.Id, product.Name } into g
                orderby g.Sum() descending
                select new
                {
                    g.Key.Name,
                    Value = g.Sum() ?? 0
                }
            ).Take(5).ToListAsync())
            .Select(x => new InsightRow { Name = x.Name, Value = (decimal)x.Value })
            .ToList();

            for (int i = 0; i < topProducts.Count; i++)
            {
                topProducts[i].Rank = $"{i + 1}";
                topProducts[i].DisplayValue = KpiValueFormatter.Format(topProducts[i].Value, KpiValueKind.Money, culture, digits);
            }
            dto.BestSellingProducts = topProducts;

            var alerts = await _context.Products
                .Where(p => p.TrackStock && p.IsActive)
                .Select(p => new
                {
                    p.Name,
                    Current = _context.ProductStocks
                        .Where(s => s.ProductId == p.Id)
                        .Sum(s => (double?)s.Quantity.Value) ?? 0,
                    Minimum = p.MinimumStock.Value
                })
                .Where(x => x.Current <= (double)x.Minimum)
                .OrderBy(x => x.Current)
                .Take(5)
                .ToListAsync();

            dto.StockAlerts = alerts.Select((a, i) => new InsightRow
            {
                Rank = $"{i + 1}",
                Name = a.Name,
                Value = (decimal)a.Current,
                DisplayValue = $"{AppStrings.MinimumLabel} {KpiValueFormatter.Format(a.Minimum, KpiValueKind.Count, culture)}"
            }).ToList();

            var custGroups = await _context.SalesInvoices
                .Where(s => !s.IsVoided)
                .GroupBy(s => s.CustomerId)
                .OrderByDescending(g => g.Sum(s => (double?)s.Total.Amount))
                .Take(5)
                .Select(g => new { CustomerId = g.Key, Value = g.Sum(s => (double?)s.Total.Amount) ?? 0 })
                .ToListAsync();

            var custIds = custGroups.Select(g => g.CustomerId).Distinct().ToList();
            var custNames = await _context.Customers.AsNoTracking()
                .Where(c => custIds.Contains(c.Id))
                .Select(c => new { c.Id, c.FullName })
                .ToDictionaryAsync(x => x.Id, x => x.FullName);

            var topCust = custGroups
                .Select(x => new InsightRow
                {
                    Name = ResolveParty(x.CustomerId, custNames, AppStrings.WalkInCustomer),
                    Value = (decimal)x.Value
                })
                .ToList();

            for (int i = 0; i < topCust.Count; i++)
            {
                topCust[i].Rank = $"{i + 1}";
                topCust[i].DisplayValue = KpiValueFormatter.Format(topCust[i].Value, KpiValueKind.Money, culture, digits);
            }
            dto.TopCustomers = topCust;

            var suppGroups = await _context.PurchaseInvoices
                .GroupBy(p => p.SupplierId)
                .OrderByDescending(g => g.Sum(p => (double?)p.Total))
                .Take(5)
                .Select(g => new { SupplierId = g.Key, Value = g.Sum(p => (double?)p.Total) ?? 0 })
                .ToListAsync();

            var suppIds = suppGroups.Select(g => g.SupplierId).Distinct().ToList();
            var suppNames = await _context.Suppliers.AsNoTracking()
                .Where(s => suppIds.Contains(s.Id))
                .Select(s => new { s.Id, s.Name })
                .ToDictionaryAsync(x => x.Id, x => x.Name);

            var topSupp = suppGroups
                .Select(x => new InsightRow
                {
                    Name = ResolveParty(x.SupplierId, suppNames, x.SupplierId.ToString()),
                    Value = (decimal)x.Value
                })
                .ToList();

            for (int i = 0; i < topSupp.Count; i++)
            {
                topSupp[i].Rank = $"{i + 1}";
                topSupp[i].DisplayValue = KpiValueFormatter.Format(topSupp[i].Value, KpiValueKind.Money, culture, digits);
            }
            dto.TopSuppliers = topSupp;
        }
    }
}

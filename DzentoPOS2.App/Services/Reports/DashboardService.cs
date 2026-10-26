using DzentoPOS2.App.Services.Reports.Interfaces;
using DzentoPOS2.App.Services.Systems;
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

        public DashboardService(
            AppDbContext context,
            RegionalContext regional,
            IBusinessClock clock)
        {
            _context = context;
            _regional = regional;
            _clock = clock;
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync()
        {
            // The day and month boundaries follow the time zone configured in the
            // settings; the values compared against the database stay in UTC.
            var localNow = _clock.Now;
            var (todayStart, tomorrowStart) = _clock.TodayRangeUtc();
            var yesterdayStart = _regional.StartOfLocalDayUtc(localNow.Date.AddDays(-1));
            var monthStart = _regional.StartOfLocalMonthUtc();
            var nextMonthStart = _regional.ToUtcSafe(new DateTime(localNow.Year, localNow.Month, 1).AddMonths(1));
            var lastMonthStart = _regional.ToUtcSafe(new DateTime(localNow.Year, localNow.Month, 1).AddMonths(-1));
            var thirtyDaysAgo = _regional.StartOfLocalDayUtc(localNow.Date.AddDays(-29));

            var dto = new DashboardSummaryDto();

            await ComputeKpiCards(dto, todayStart, tomorrowStart, yesterdayStart, monthStart, nextMonthStart, lastMonthStart);
            await ComputeChartData(dto, thirtyDaysAgo, tomorrowStart, localNow);
            await ComputeRecentTransactions(dto);
            await ComputeInsights(dto);

            return dto;
        }

        private async Task ComputeKpiCards(DashboardSummaryDto dto,
            DateTime todayStart, DateTime tomorrowStart,
            DateTime yesterdayStart,
            DateTime monthStart, DateTime nextMonthStart, DateTime lastMonthStart)
        {
            // Sales — invoice value (Total), periods expressed as half-open ranges.
            // Note: SQLite cannot SUM decimal columns directly, so amounts are cast to
            // double in SQL (translated to CAST(x AS REAL)) and converted back on read.
            // One grouped query computes every sales KPI instead of six round trips.
            var salesStats = await _context.SalesInvoices
                .Where(s => !s.IsVoided)
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

            // Revenue / COGS aggregated in SQL (no full table materialization).
            var grossRevenue = await (
                from item in _context.SalesInvoiceItems
                join invoice in _context.SalesInvoices
                    on item.SalesInvoiceId equals invoice.Id
                where invoice.CreatedAt >= monthStart && invoice.CreatedAt < nextMonthStart
                    && !invoice.IsVoided
                select (double?)item.Total
            ).SumAsync() is double gr ? (decimal)gr : 0m;

            var cogs = await (
                from item in _context.SalesInvoiceItems
                join invoice in _context.SalesInvoices
                    on item.SalesInvoiceId equals invoice.Id
                join product in _context.Products
                    on item.ProductId equals product.Id
                where invoice.CreatedAt >= monthStart && invoice.CreatedAt < nextMonthStart
                    && !invoice.IsVoided
                select (double?)(item.Quantity * product.CostPrice.Amount)
            ).SumAsync() is double cg ? (decimal)cg : 0m;

            var monthlyReturns = await _context.SalesReturns
                .Where(r => r.CreatedAt >= monthStart && r.CreatedAt < nextMonthStart)
                .SumAsync(r => (double?)r.TotalAmount) is double mr ? (decimal)mr : 0m;

            var monthlyExpenses = await _context.Expenses
                .Where(e => e.ExpenseDate >= monthStart && e.ExpenseDate < nextMonthStart)
                .SumAsync(e => (double?)e.Amount) is double me ? (decimal)me : 0m;

            // Gross profit = net revenue (sales − returns) − cost of goods sold.
            var netRevenue = grossRevenue - monthlyReturns;
            var grossProfit = netRevenue - cogs;

            // Actual profit = COGS-based gross profit − operating expenses.
            var actualProfit = grossProfit - monthlyExpenses;

            var grossMargin = netRevenue > 0 ? Math.Round(grossProfit / netRevenue * 100, 1) : 0;
            var netMargin = netRevenue > 0 ? Math.Round(actualProfit / netRevenue * 100, 1) : 0;

            SetCardData(dto.ExpectedProfit, grossProfit, 0, KpiValueKind.Money);
            SetCardData(dto.ActualProfit, actualProfit, 0, KpiValueKind.Money);

            // Profit cards show the real margin (never a bare "%هامش" with no number).
            dto.ExpectedProfit.TrendKind = netRevenue > 0 ? KpiTrendKind.Margin : KpiTrendKind.None;
            dto.ExpectedProfit.TrendPercent = grossMargin;
            dto.ActualProfit.TrendKind = netRevenue > 0 ? KpiTrendKind.Margin : KpiTrendKind.None;
            dto.ActualProfit.TrendPercent = netMargin;

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

        private async Task ComputeChartData(DashboardSummaryDto dto, DateTime thirtyDaysAgoUtc, DateTime tomorrowStartUtc, DateTime localNow)
        {
            var sixMonthsAgo = _regional.ToUtcSafe(new DateTime(localNow.Year, localNow.Month, 1).AddMonths(-5));

            // A single read over the last six months feeds both the daily trend and
            // the month-over-month comparison.
            var salesRows = await _context.SalesInvoices
                .Where(s => !s.IsVoided && s.CreatedAt >= sixMonthsAgo && s.CreatedAt < tomorrowStartUtc)
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

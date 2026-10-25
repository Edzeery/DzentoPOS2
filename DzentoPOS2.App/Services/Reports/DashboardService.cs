using DzentoPOS2.App.Services.Reports.Interfaces;
using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Shared.Contracts.DTOs.Reports;
using DzentoPOS2.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

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

            SetCardData(dto.SalesToday, (decimal)(salesStats?.TodayTotal ?? 0), (decimal)(salesStats?.YesterdayTotal ?? 0));
            SetCardData(dto.SalesCount, salesStats?.TodayCount ?? 0, salesStats?.YesterdayCount ?? 0);

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

            SetCardData(dto.PurchasesToday, (decimal)(purchaseStats?.TodayTotal ?? 0), (decimal)(purchaseStats?.YesterdayTotal ?? 0));
            SetCardData(dto.PurchasesCount, purchaseStats?.TodayCount ?? 0, purchaseStats?.YesterdayCount ?? 0);
            SetCardData(dto.MonthlySales, (decimal)(salesStats?.MonthTotal ?? 0), (decimal)(salesStats?.LastMonthTotal ?? 0));
            SetCardData(dto.MonthlyPurchases, (decimal)(purchaseStats?.MonthTotal ?? 0), (decimal)(purchaseStats?.LastMonthTotal ?? 0));

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

            SetCardData(dto.ExpectedProfit, grossProfit, 0);
            SetCardData(dto.ActualProfit, actualProfit, 0);
            dto.ExpectedProfit.TrendDisplay = $"هامش {grossMargin}%";
            dto.ExpectedProfit.TrendPercent = grossMargin;
            dto.ActualProfit.TrendDisplay = $"هامش {netMargin}%";
            dto.ActualProfit.TrendPercent = netMargin;

            var productStats = await _context.Products
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Active = g.Count(p => p.IsActive),
                    LastMonth = g.Count(p => p.IsActive && p.CreatedAt < monthStart)
                })
                .FirstOrDefaultAsync();

            SetCardData(dto.ActiveProducts, productStats?.Active ?? 0, productStats?.LastMonth ?? 0);

            // Low stock — products whose total warehouse stock is at/below reorder level.
            var lowStockCount = await _context.Products
                .Where(p => p.IsActive && p.TrackStock)
                .Where(p => _context.ProductStocks
                    .Where(s => s.ProductId == p.Id)
                    .Sum(s => (double?)s.Quantity.Value) <= (double)p.MinimumStock.Value)
                .CountAsync();

            SetCardData(dto.LowStock, lowStockCount, 0);

            var customerStats = await _context.Customers
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Active = g.Count(c => c.IsActive),
                    LastMonth = g.Count(c => c.IsActive && c.CreatedAt < monthStart)
                })
                .FirstOrDefaultAsync();

            SetCardData(dto.ActiveCustomers, customerStats?.Active ?? 0, customerStats?.LastMonth ?? 0);

            var supplierStats = await _context.Suppliers
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Active = g.Count(s => s.IsActive),
                    LastMonth = g.Count(s => s.IsActive && s.CreatedAt < monthStart)
                })
                .FirstOrDefaultAsync();

            SetCardData(dto.ActiveSuppliers, supplierStats?.Active ?? 0, supplierStats?.LastMonth ?? 0);
        }

        private static string MonthKey(DateTime local) => $"{local.Year}-{local.Month:D2}";

        private static void SetCardData(DashboardCardData card, decimal current, decimal previous)
        {
            card.Value = current;
            card.DisplayValue = FormatValue(current);
            card.TrendPercent = previous != 0 ? Math.Round((current - previous) / previous * 100, 1) : 0;
            card.TrendDisplay = card.TrendPercent >= 0
                ? $"+{card.TrendPercent}%"
                : $"{card.TrendPercent}%";
        }

        private static string FormatValue(decimal value)
        {
            if (value >= 1_000_000)
                return $"{value / 1_000_000:N2}M";
            if (value >= 1_000)
                return $"{value:N0}";
            return $"{value:N2}";
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

            var culture = new System.Globalization.CultureInfo("ar-SA");
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
                    Category = string.IsNullOrWhiteSpace(c.Category) ? "أخرى" : c.Category,
                    Value = (decimal)c.Total
                });
            }
        }

        private async Task ComputeRecentTransactions(DashboardSummaryDto dto)
        {
            var sales = await _context.SalesInvoices
                .Where(s => !s.IsVoided)
                .OrderByDescending(s => s.CreatedAt)
                .Take(5)
                .Select(s => new TransactionRow
                {
                    Id = s.Id,
                    Type = "بيع",
                    Number = s.InvoiceNumber.Value,
                    Party = s.CustomerId.ToString(),
                    Amount = s.Total.Amount,
                    Date = s.CreatedAt,
                    Status = s.PaidAmount.Amount >= s.Total.Amount ? "مدفوع" : "معلق"
                })
                .ToListAsync();

            var purchases = await _context.PurchaseInvoices
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .Select(p => new TransactionRow
                {
                    Id = p.Id,
                    Type = "شراء",
                    Number = p.InvoiceNumber,
                    Party = p.SupplierId.ToString(),
                    Amount = p.Total,
                    Date = p.CreatedAt,
                    Status = p.PaidAmount >= p.Total ? "مدفوع" : "معلق"
                })
                .ToListAsync();

            dto.RecentTransactions = sales
                .Concat(purchases)
                .OrderByDescending(t => t.Date)
                .Take(8)
                .ToList();
        }

        private async Task ComputeInsights(DashboardSummaryDto dto)
        {
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
                topProducts[i].DisplayValue = topProducts[i].Value.ToString("N0");
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
                DisplayValue = $"الحد الأدنى {a.Minimum:N0}"
            }).ToList();

            var topCust = (await _context.SalesInvoices
                .Where(s => !s.IsVoided)
                .GroupBy(s => s.CustomerId)
                .OrderByDescending(g => g.Sum(s => (double?)s.Total.Amount))
                .Take(5)
                .Select(g => new { Name = g.Key.ToString(), Value = g.Sum(s => (double?)s.Total.Amount) ?? 0 })
                .ToListAsync())
                .Select(x => new InsightRow { Name = x.Name, Value = (decimal)x.Value })
                .ToList();

            for (int i = 0; i < topCust.Count; i++)
            {
                topCust[i].Rank = $"{i + 1}";
                topCust[i].DisplayValue = topCust[i].Value.ToString("N0");
            }
            dto.TopCustomers = topCust;

            var topSupp = (await _context.PurchaseInvoices
                .GroupBy(p => p.SupplierId)
                .OrderByDescending(g => g.Sum(p => (double?)p.Total))
                .Take(5)
                .Select(g => new { Name = g.Key.ToString(), Value = g.Sum(p => (double?)p.Total) ?? 0 })
                .ToListAsync())
                .Select(x => new InsightRow { Name = x.Name, Value = (decimal)x.Value })
                .ToList();

            for (int i = 0; i < topSupp.Count; i++)
            {
                topSupp[i].Rank = $"{i + 1}";
                topSupp[i].DisplayValue = topSupp[i].Value.ToString("N0");
            }
            dto.TopSuppliers = topSupp;
        }
    }
}

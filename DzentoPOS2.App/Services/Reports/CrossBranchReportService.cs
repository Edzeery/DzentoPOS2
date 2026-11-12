using DzentoPOS2.App.Authorization;
using DzentoPOS2.App.Services.Reports.Interfaces;
using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.Core.Constants;
using DzentoPOS2.Core.Services.Sales;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Shared.Contracts.DTOs.Reports;
using DzentoPOS2.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.App.Services.Reports;

/// <summary>
/// Cross-branch unified report. For the selected period it returns one row per
/// branch (sales, returns, net sales, purchases, expenses, gross profit and net
/// profit) plus a grand total, so a manager can compare branches at a glance.
///
/// <para>
/// The figures mirror the dashboard: sales expense the invoice total, while the
/// profit columns follow <see cref="ProfitCalculator"/> over the frozen line
/// values (revenue net of discount, cost from the stored unit cost, and the
/// profit impact of returns), so the two screens can never disagree. The report
/// is read-only.
/// </para>
///
/// <para>
/// Access to every branch requires <c>BranchesViewAll</c>; without it the
/// report silently falls back to the caller's active branch. The whole
/// operation requires <c>ReportsView</c>.
/// </para>
/// </summary>
public class CrossBranchReportService : ICrossBranchReportService
{
    private readonly AppDbContext _context;
    private readonly RegionalContext _regional;
    private readonly IBusinessClock _clock;
    private readonly IActiveBranchContext _activeBranch;
    private readonly IAuthorizationGuard _guard;

    public CrossBranchReportService(
        AppDbContext context,
        RegionalContext regional,
        IBusinessClock clock,
        IActiveBranchContext activeBranch,
        IAuthorizationGuard guard)
    {
        _context = context;
        _regional = regional;
        _clock = clock;
        _activeBranch = activeBranch;
        _guard = guard;
    }

    public async Task<CrossBranchReportDto> GetAsync(CrossBranchReportFilterDto? filter = null)
    {
        filter ??= new CrossBranchReportFilterDto();

        await _guard.DemandAsync(Permissions.ReportsView);

        // Viewing every branch is a privilege; the flag is a request that is
        // only honoured for a user who actually holds BranchesViewAll.
        var viewAll = filter.ViewAllBranches
            && await _guard.IsAllowedAsync(Permissions.BranchesViewAll);
        var activeBranchId = _activeBranch.ActiveBranchId;

        var resolved = DashboardRangeResolver.Resolve(_clock, new DashboardFilterDto
        {
            Preset = filter.Preset,
            CustomFrom = filter.CustomFrom,
            CustomTo = filter.CustomTo
        });

        var start = resolved.StartUtc;
        var end = resolved.EndUtc;
        var digits = _regional.CurrencyDecimalDigits;

        var dto = new CrossBranchReportDto
        {
            RangeStartUtc = start,
            RangeEndUtc = end,
            ViewAllBranches = viewAll
        };

        var branches = await _context.Branches
            .AsNoTracking()
            .Where(b => viewAll || b.Id == activeBranchId)
            .OrderByDescending(b => b.IsMainBranch)
            .ThenBy(b => b.Name)
            .Select(b => new { b.Id, b.Name, b.IsMainBranch, b.IsActive })
            .ToListAsync();

        var rows = new List<BranchReportRowDto>(branches.Count);
        var rowById = new Dictionary<Guid, BranchReportRowDto>(branches.Count);

        foreach (var branch in branches)
        {
            var row = new BranchReportRowDto
            {
                BranchId = branch.Id,
                BranchName = branch.Name,
                IsMainBranch = branch.IsMainBranch,
                IsActive = branch.IsActive
            };

            rows.Add(row);
            rowById[branch.Id] = row;
        }

        await FillSalesAsync(rowById, viewAll, activeBranchId, start, end, digits);
        await FillPurchasesAsync(rowById, viewAll, activeBranchId, start, end, digits);
        await FillExpensesAsync(rowById, viewAll, activeBranchId, start, end, digits);
        await FillProfitAsync(rowById, viewAll, activeBranchId, start, end, digits);

        dto.Rows = rows;
        dto.Total = BuildTotal(rows, digits);
        return dto;
    }

    private async Task FillSalesAsync(
        Dictionary<Guid, BranchReportRowDto> rows,
        bool viewAll,
        Guid activeBranchId,
        DateTime start,
        DateTime end,
        int digits)
    {
        var sales = await _context.SalesInvoices
            .AsNoTracking()
            .Where(s => (viewAll || s.BranchId == activeBranchId)
                && !s.IsVoided
                && s.CreatedAt >= start && s.CreatedAt < end)
            .GroupBy(s => s.BranchId)
            .Select(g => new
            {
                BranchId = g.Key,
                Count = g.Count(),
                Total = g.Sum(s => (double?)s.Total.Amount) ?? 0
            })
            .ToListAsync();

        foreach (var s in sales)
        {
            if (!rows.TryGetValue(s.BranchId, out var row))
                continue;

            row.SalesCount = s.Count;
            row.SalesTotal = ProfitCalculator.Round((decimal)s.Total, digits);
        }

        var returns = await (
            from r in _context.SalesReturns.AsNoTracking()
            join inv in _context.SalesInvoices.AsNoTracking() on r.SalesInvoiceId equals inv.Id
            where (viewAll || inv.BranchId == activeBranchId)
                && !inv.IsVoided
                && r.CreatedAt >= start && r.CreatedAt < end
            group (double?)r.TotalAmount by inv.BranchId into g
            select new { BranchId = g.Key, Total = g.Sum() ?? 0 }
        ).ToListAsync();

        foreach (var r in returns)
        {
            if (rows.TryGetValue(r.BranchId, out var row))
                row.ReturnsTotal = ProfitCalculator.Round((decimal)r.Total, digits);
        }

        foreach (var row in rows.Values)
            row.NetSales = ProfitCalculator.Round(row.SalesTotal - row.ReturnsTotal, digits);
    }

    private async Task FillPurchasesAsync(
        Dictionary<Guid, BranchReportRowDto> rows,
        bool viewAll,
        Guid activeBranchId,
        DateTime start,
        DateTime end,
        int digits)
    {
        var purchases = await _context.PurchaseInvoices
            .AsNoTracking()
            .Where(p => (viewAll || p.BranchId == activeBranchId)
                && p.CreatedAt >= start && p.CreatedAt < end)
            .GroupBy(p => p.BranchId)
            .Select(g => new
            {
                BranchId = g.Key,
                Count = g.Count(),
                Total = g.Sum(p => (double?)p.Total) ?? 0
            })
            .ToListAsync();

        foreach (var p in purchases)
        {
            if (!rows.TryGetValue(p.BranchId, out var row))
                continue;

            row.PurchasesCount = p.Count;
            row.PurchasesTotal = ProfitCalculator.Round((decimal)p.Total, digits);
        }
    }

    private async Task FillExpensesAsync(
        Dictionary<Guid, BranchReportRowDto> rows,
        bool viewAll,
        Guid activeBranchId,
        DateTime start,
        DateTime end,
        int digits)
    {
        var expenses = await _context.Expenses
            .AsNoTracking()
            .Where(e => (viewAll || e.BranchId == activeBranchId)
                && e.ExpenseDate >= start && e.ExpenseDate < end)
            .GroupBy(e => e.BranchId)
            .Select(g => new { BranchId = g.Key, Total = g.Sum(e => (double?)e.Amount) ?? 0 })
            .ToListAsync();

        foreach (var e in expenses)
        {
            if (rows.TryGetValue(e.BranchId, out var row))
                row.ExpensesTotal = ProfitCalculator.Round((decimal)e.Total, digits);
        }
    }

    private async Task FillProfitAsync(
        Dictionary<Guid, BranchReportRowDto> rows,
        bool viewAll,
        Guid activeBranchId,
        DateTime start,
        DateTime end,
        int digits)
    {
        // Frozen line values for every non-voided invoice in range.
        var itemRows = await (
            from item in _context.SalesInvoiceItems.AsNoTracking()
            join inv in _context.SalesInvoices.AsNoTracking() on item.SalesInvoiceId equals inv.Id
            where (viewAll || inv.BranchId == activeBranchId)
                && !inv.IsVoided
                && inv.CreatedAt >= start && inv.CreatedAt < end
            select new
            {
                inv.BranchId,
                item.SalesInvoiceId,
                item.Quantity,
                item.Price,
                item.UnitCost,
                item.ListPrice,
                item.DiscountAmount
            }
        ).ToListAsync();

        var invoices = await _context.SalesInvoices
            .AsNoTracking()
            .Where(i => (viewAll || i.BranchId == activeBranchId)
                && !i.IsVoided
                && i.CreatedAt >= start && i.CreatedAt < end)
            .Select(i => new { i.Id, i.BranchId, Fees = i.FeesAmount.Amount })
            .ToListAsync();

        var feeByInvoice = invoices.ToDictionary(i => i.Id, i => i.Fees);

        // Revenue / cost / fees accumulated per branch from the frozen lines.
        var revenueByBranch = new Dictionary<Guid, (decimal Revenue, decimal Cost, decimal Fees)>();

        foreach (var branchGroup in itemRows.GroupBy(x => x.BranchId))
        {
            decimal revenue = 0m, cost = 0m, fees = 0m;

            foreach (var invoice in branchGroup.GroupBy(x => x.SalesInvoiceId))
            {
                var lines = invoice
                    .Select(x => new ProfitLine(
                        x.Quantity,
                        x.Price,
                        x.ListPrice,
                        x.UnitCost,
                        x.DiscountAmount))
                    .ToList();

                var profit = ProfitCalculator.CalculateInvoice(
                    lines,
                    fees: feeByInvoice.TryGetValue(invoice.Key, out var f) ? f : 0m,
                    digits: digits);

                revenue += profit.Revenue;
                cost += profit.Cost;
                fees += profit.Fees;
            }

            revenueByBranch[branchGroup.Key] = (revenue, cost, fees);
        }

        // Returns reduce gross profit by their profit impact (returned net value
        // minus the cost of the goods that came back).
        var returnsEffect = await ComputeReturnsNetEffectByBranchAsync(
            viewAll, activeBranchId, start, end, digits);

        // Every branch in scope gets a figure, even one with no sales.
        foreach (var row in rows.Values)
        {
            var acc = revenueByBranch.TryGetValue(row.BranchId, out var a) ? a : default;
            var effect = returnsEffect.GetValueOrDefault(row.BranchId);

            var gross = ProfitCalculator.CalculateGrossProfit(acc.Revenue, acc.Cost, effect);
            var actual = ProfitCalculator.CalculateActualProfit(gross, acc.Fees);
            var net = ProfitCalculator.CalculateNetProfit(actual, row.ExpensesTotal);

            row.GrossProfit = ProfitCalculator.Round(gross, digits);
            row.NetProfit = ProfitCalculator.Round(net, digits);
        }
    }

    private async Task<Dictionary<Guid, decimal>> ComputeReturnsNetEffectByBranchAsync(
        bool viewAll,
        Guid activeBranchId,
        DateTime start,
        DateTime end,
        int digits)
    {
        var returnRows = await (
            from r in _context.SalesReturns.AsNoTracking()
            join i in _context.SalesReturnItems.AsNoTracking() on r.Id equals i.SalesReturnId
            join inv in _context.SalesInvoices.AsNoTracking() on r.SalesInvoiceId equals inv.Id
            where (viewAll || inv.BranchId == activeBranchId)
                && !inv.IsVoided
                && r.CreatedAt >= start && r.CreatedAt < end
            select new
            {
                inv.BranchId,
                r.SalesInvoiceId,
                i.ProductId,
                i.Quantity,
                i.Total
            }
        ).ToListAsync();

        var effect = new Dictionary<Guid, decimal>();
        if (returnRows.Count == 0)
            return effect;

        var returnInvoiceIds = returnRows
            .Select(r => r.SalesInvoiceId)
            .Distinct()
            .ToList();

        var originalRows = (await _context.SalesInvoiceItems
            .AsNoTracking()
            .Where(item => returnInvoiceIds.Contains(item.SalesInvoiceId))
            .Select(item => new
            {
                item.SalesInvoiceId,
                item.ProductId,
                item.Quantity,
                item.UnitCost,
                item.DiscountAmount
            })
            .ToListAsync())
            .GroupBy(x => new { x.SalesInvoiceId, x.ProductId })
            .Select(g =>
            {
                var qty = g.Sum(x => x.Quantity);
                return (
                    g.Key.SalesInvoiceId,
                    g.Key.ProductId,
                    Qty: qty,
                    UnitCost: qty == 0 ? 0m : g.Sum(x => x.Quantity * x.UnitCost) / qty,
                    DiscountAmount: g.Sum(x => x.DiscountAmount));
            })
            .ToList();

        var originalLookup = originalRows
            .ToDictionary(x => (x.SalesInvoiceId, x.ProductId));

        var missingProductIds = returnRows
            .Select(r => r.ProductId)
            .Distinct()
            .Where(pid => !originalLookup.Keys.Any(k => k.ProductId == pid))
            .ToList();

        var productCosts = missingProductIds.Count == 0
            ? new Dictionary<Guid, decimal>()
            : await _context.Products
                .AsNoTracking()
                .Where(p => missingProductIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.CostPrice.Amount);

        foreach (var rr in returnRows)
        {
            decimal lineEffect;

            if (originalLookup.TryGetValue((rr.SalesInvoiceId, rr.ProductId), out var orig))
            {
                var discountShare = orig.Qty > 0
                    ? orig.DiscountAmount * rr.Quantity / orig.Qty
                    : 0m;
                var returnLineNet = rr.Total - discountShare;
                lineEffect = returnLineNet - rr.Quantity * orig.UnitCost;
            }
            else
            {
                var unitCost = productCosts.TryGetValue(rr.ProductId, out var cost) ? cost : 0m;
                lineEffect = rr.Total - rr.Quantity * unitCost;
            }

            effect[rr.BranchId] = effect.GetValueOrDefault(rr.BranchId) + lineEffect;
        }

        return effect;
    }

    private static BranchReportRowDto BuildTotal(
        IReadOnlyCollection<BranchReportRowDto> rows,
        int digits)
    {
        var total = new BranchReportRowDto
        {
            BranchName = AppStrings.Total,
            IsMainBranch = false,
            IsActive = true
        };

        foreach (var row in rows)
        {
            total.SalesCount += row.SalesCount;
            total.SalesTotal += row.SalesTotal;
            total.ReturnsTotal += row.ReturnsTotal;
            total.NetSales += row.NetSales;
            total.PurchasesCount += row.PurchasesCount;
            total.PurchasesTotal += row.PurchasesTotal;
            total.ExpensesTotal += row.ExpensesTotal;
            total.GrossProfit += row.GrossProfit;
            total.NetProfit += row.NetProfit;
        }

        total.SalesTotal = ProfitCalculator.Round(total.SalesTotal, digits);
        total.ReturnsTotal = ProfitCalculator.Round(total.ReturnsTotal, digits);
        total.NetSales = ProfitCalculator.Round(total.NetSales, digits);
        total.PurchasesTotal = ProfitCalculator.Round(total.PurchasesTotal, digits);
        total.ExpensesTotal = ProfitCalculator.Round(total.ExpensesTotal, digits);
        total.GrossProfit = ProfitCalculator.Round(total.GrossProfit, digits);
        total.NetProfit = ProfitCalculator.Round(total.NetProfit, digits);

        return total;
    }
}

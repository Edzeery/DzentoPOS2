using DzentoPOS2.App.Services.Events;
using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.Core.Services.Inventory;
using DzentoPOS2.Core.Services.Sales;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Shared.Contracts.DTOs.Inventory;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DzentoPOS2.App.Services.Inventory
{
    public sealed class InventoryValuationService : IInventoryValuationService, IDisposable
    {
        private readonly AppDbContext _context;
        private readonly IBusinessClock _clock;
        private readonly ISalesEventPublisher _salesEvents;
        private readonly IInventoryChangedNotifier _notifier;
        private readonly IActiveBranchContext _activeBranch;

        private InventoryValuationDto? _cache;
        private DateTime _cacheAtUtc;
        private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(30);
        private readonly object _lock = new();

        public InventoryValuationService(
            AppDbContext context,
            IBusinessClock clock,
            ISalesEventPublisher salesEvents,
            IInventoryChangedNotifier notifier,
            IActiveBranchContext activeBranch)
        {
            _context = context;
            _clock = clock;
            _salesEvents = salesEvents;
            _notifier = notifier;
            _activeBranch = activeBranch;
            _salesEvents.SaleCompleted += (_, _) => InvalidateCache();
            _salesEvents.SaleVoided += (_, _) => InvalidateCache();
            _notifier.Changed += (_, _) => InvalidateCache();
        }

        public void InvalidateCache()
        {
            lock (_lock) { _cache = null; }
        }

        public async Task<InventoryValuationDto> GetValuationAsync(CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_cache is not null && (DateTime.UtcNow - _cacheAtUtc) < CacheTtl)
                    return _cache;
            }

            var dto = await ComputeAsync(cancellationToken);

            lock (_lock)
            {
                _cache = dto;
                _cacheAtUtc = DateTime.UtcNow;
            }
            return dto;
        }

        private async Task<InventoryValuationDto> ComputeAsync(CancellationToken cancellationToken)
        {
            var cutoff = _clock.UtcNow.AddDays(-90);

            // 1. Main valuation query: stock aggregated per variant (VariantId level).
            var branchId = _activeBranch.ActiveBranchId;
            var warehouseIds = await _context.Warehouses
                .AsNoTracking()
                .Where(w => w.BranchId == branchId && w.IsActive)
                .Select(w => w.Id)
                .ToListAsync(cancellationToken);

            var stockRows = await (
                from s in _context.ProductStocks
                join v in _context.ProductVariants on s.VariantId equals v.Id
                join p in _context.Products on v.ProductId equals p.Id
                where warehouseIds.Contains(s.WarehouseId) && !p.IsDeleted && p.IsActive && v.IsActive && p.TrackStock && v.TrackStock
                group s by new
                {
                    v.Id,
                    Sale = v.SalePrice.Amount,
                    Cost = v.CostPrice.Amount,
                    CategoryId = p.CategoryId
                } into g
                select new
                {
                    VariantId = g.Key.Id,
                    Qty = g.Sum(x => (double?)x.Quantity.Value) ?? 0d,
                    Sale = g.Key.Sale,
                    Cost = g.Key.Cost,
                    CategoryId = g.Key.CategoryId
                }
            ).ToListAsync(cancellationToken);

            var products = stockRows
                .Where(r => r.Qty > 0)
                .Select(r => new InventoryProductInput(
                    r.VariantId, r.Sale, r.Cost, true, true, r.CategoryId))
                .ToList();
            var stocks = stockRows
                .Where(r => r.Qty > 0)
                .Select(r => new InventoryStockInput(r.VariantId, (decimal)r.Qty))
                .ToList();

            // 2. Discount factors: last 90 days non-voided sales.
            var discountRows = await (
                from item in _context.SalesInvoiceItems
                join inv in _context.SalesInvoices on item.SalesInvoiceId equals inv.Id
                where inv.BranchId == branchId && !inv.IsVoided && inv.CreatedAt >= cutoff
                select new
                {
                    item.VariantId,
                    item.DiscountAmount,
                    item.ListPrice,
                    item.Quantity,
                    ProductId = item.ProductId
                }
            ).ToListAsync(cancellationToken);

            // map variant -> category via products (already loaded in stockRows; load all variants' products)
            var variantCategory = await _context.ProductVariants
                .AsNoTracking()
                .Select(v => new { v.Id, v.Product.CategoryId })
                .ToDictionaryAsync(x => x.Id, x => x.CategoryId, cancellationToken);

            // per-variant sample counts
            var perVariant = discountRows
                .GroupBy(r => r.VariantId)
                .ToDictionary(g => g.Key, g => new
                {
                    Count = g.Count(),
                    Discount = g.Sum(x => x.DiscountAmount),
                    Revenue = g.Sum(x => x.ListPrice * x.Quantity)
                });

            var perCategory = discountRows
                .GroupBy(r => variantCategory.TryGetValue(r.VariantId, out var c) ? c : Guid.Empty)
                .ToDictionary(g => g.Key, g => new
                {
                    Count = g.Count(),
                    Discount = g.Sum(x => x.DiscountAmount),
                    Revenue = g.Sum(x => x.ListPrice * x.Quantity)
                });

            var storeCount = discountRows.Count;
            var storeDiscount = discountRows.Sum(x => x.DiscountAmount);
            var storeRevenue = discountRows.Sum(x => x.ListPrice * x.Quantity);

            decimal StoreRate() => storeRevenue > 0 ? storeDiscount / storeRevenue : 0m;

            Func<Guid, decimal> discountFactor = (variantId) =>
            {
                if (perVariant.TryGetValue(variantId, out var pv) && pv.Count >= 5 && pv.Revenue > 0)
                    return pv.Discount / pv.Revenue;
                var catId = variantCategory.TryGetValue(variantId, out var c) ? c : Guid.Empty;
                if (perCategory.TryGetValue(catId, out var pc) && pc.Count >= 5 && pc.Revenue > 0)
                    return pc.Discount / pc.Revenue;
                return StoreRate();
            };

            // 3. Fee factor: last 90 days non-voided invoices.
            var feeRows = await _context.SalesInvoices
                .AsNoTracking()
                .Where(i => i.BranchId == branchId && !i.IsVoided && i.CreatedAt >= cutoff)
                .Select(i => new { Fees = i.FeesAmount.Amount, Subtotal = i.Subtotal.Amount, Discount = i.DiscountAmount.Amount })
                .ToListAsync(cancellationToken);

            var totalFees = feeRows.Sum(x => x.Fees);
            var totalNetRevenue = feeRows.Sum(x => x.Subtotal - x.Discount);
            var feeFactor = totalNetRevenue > 0 ? totalFees / totalNetRevenue : 0m;

            var result = InventoryProfitCalculator.Calculate(products, stocks, discountFactor, feeFactor);

            var dto = new InventoryValuationDto
            {
                TotalQuantity = stocks.Sum(s => s.Quantity),
                CostValue = result.CostValue,
                RetailValue = result.RetailValue,
                PotentialProfit = result.PotentialProfit,
                PotentialMarginPct = result.PotentialMarginPct,
                ExpectedProfit = result.RealisticProfit,
                AtRiskProfit = result.AtRiskProfit,
                WarningNoSalePrice = result.WarningNoSalePrice,
                WarningNegativeMargin = result.WarningNegativeMargin,
                WarningZeroCost = result.WarningZeroCost,
                DiscountFactorAvg = storeRevenue > 0 ? storeDiscount / storeRevenue : 0m,
                FeeFactor = feeFactor
            };

            return dto;
        }

        public void Dispose()
        {
            _salesEvents.SaleCompleted -= (_, _) => InvalidateCache();
            // (event handler removal by lambda identity is best-effort; cache TTL covers staleness)
        }
    }
}
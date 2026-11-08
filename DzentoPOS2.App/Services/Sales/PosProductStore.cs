using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DzentoPOS2.App.Services.Sales;

public interface IPosProductStore
{
    Task<IReadOnlyList<PosProductItem>> GetAllAsync();
    Task<PosProductItem?> FindByVariantIdAsync(Guid variantId);
    Task<IReadOnlyList<PosProductItem>> FindByBarcodeAsync(string barcode);
    void Invalidate();
    void UpdateStock(Guid variantId, decimal delta);
}

public class PosProductStore : IPosProductStore, IDisposable
{
    private readonly AppDbContext _context;
    private readonly ICatalogChangeNotifier _notifier;
    private readonly IActiveBranchContext _activeBranch;
    private readonly IServiceProvider _serviceProvider;
    private List<PosProductItem>? _cache;
    private Dictionary<string, List<PosProductItem>>? _barcodeIndex;
    private readonly object _lock = new();

    public PosProductStore(AppDbContext context, ICatalogChangeNotifier notifier, IActiveBranchContext activeBranch, IServiceProvider serviceProvider)
    {
        _context = context;
        _notifier = notifier;
        _activeBranch = activeBranch;
        _serviceProvider = serviceProvider;
        _notifier.CatalogChanged += OnCatalogChanged;
        _activeBranch.BranchChanged += OnBranchChanged;
    }

    private CancellationTokenSource? _reloadCts;

    private async void OnBranchChanged(object? sender, ActiveBranchChangedEventArgs e)
    {
        try
        {
            await ReloadStockAsync();
        }
        catch (Exception ex)
        {
            // Log but don't crash.
            System.Diagnostics.Debug.WriteLine($"PosProductStore.ReloadStock failed: {ex.Message}");
        }
    }

    public async Task ReloadStockAsync(CancellationToken ct = default)
    {
        _reloadCts?.Cancel();
        _reloadCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var token = _reloadCts.Token;

        var branchId = _activeBranch.ActiveBranchId;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var warehouseId = await context.Warehouses
                .AsNoTracking()
                .Where(w => w.BranchId == branchId && w.IsActive)
                .OrderBy(w => w.CreatedAt)
                .ThenBy(w => w.Id)
                .Select(w => (Guid?)w.Id)
                .FirstOrDefaultAsync(token);

            if (warehouseId is null || token.IsCancellationRequested)
                return;

            var variantIds = _cache?.Select(c => c.VariantId).ToList() ?? new();
            var stocks = await context.ProductStocks
                .AsNoTracking()
                .Where(s => variantIds.Contains(s.VariantId) && s.WarehouseId == warehouseId.Value)
                .GroupBy(s => s.VariantId)
                .Select(g => new { VariantId = g.Key, Total = g.Sum(s => s.Quantity.Value) })
                .ToListAsync(token);

            if (token.IsCancellationRequested)
                return;

            // Stale check: if branch changed during reload, discard.
            if (branchId != _activeBranch.ActiveBranchId)
                return;

            var stockMap = stocks.ToDictionary(s => s.VariantId, s => s.Total);

            lock (_lock)
            {
                if (_cache is null) return;
                for (int i = 0; i < _cache.Count; i++)
                {
                    var item = _cache[i];
                    var newStock = stockMap.TryGetValue(item.VariantId, out var s) ? s : 0m;
                    _cache[i] = item with { Stock = newStock };
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during rapid switches.
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"PosProductStore.ReloadStock failed: {ex.Message}");
        }
    }

    private async Task ReloadStockAsync()
    {
        try
        {
            var branchId = _activeBranch.ActiveBranchId;
            var warehouseId = await _context.Warehouses
                .AsNoTracking()
                .Where(w => w.BranchId == branchId && w.IsActive)
                .OrderBy(w => w.CreatedAt)
                .ThenBy(w => w.Id)
                .Select(w => (Guid?)w.Id)
                .FirstOrDefaultAsync();

            if (warehouseId is null) return;

            var stocks = await _context.ProductStocks
                .AsNoTracking()
                .Where(s => s.WarehouseId == warehouseId.Value)
                .GroupBy(s => s.VariantId)
                .Select(g => new { VariantId = g.Key, Total = g.Sum(s => s.Quantity.Value) })
                .ToListAsync();

            var stockMap = stocks.ToDictionary(s => s.VariantId, s => s.Total);

            lock (_lock)
            {
                if (_cache is null) return;
                for (int i = 0; i < _cache.Count; i++)
                {
                    var item = _cache[i];
                    var newStock = stockMap.TryGetValue(item.VariantId, out var s) ? s : 0m;
                    _cache[i] = item with { Stock = newStock };
                }
            }
        }
        catch { /* best effort */ }
    }

    private void OnCatalogChanged(object? sender, CatalogChangedEventArgs e)
    {
        if (e.FullInvalidation)
        {
            lock (_lock)
            {
                _cache = null;
                _barcodeIndex = null;
            }
        }
        else if (e.VariantId.HasValue && e.StockDelta != 0)
        {
            lock (_lock)
            {
                if (_cache is not null)
                {
                    var idx = _cache.FindIndex(x => x.VariantId == e.VariantId.Value);
                    if (idx >= 0)
                    {
                        var item = _cache[idx];
                        _cache[idx] = item with { Stock = item.Stock + e.StockDelta };
                    }
                }
            }
        }
    }

    public async Task<IReadOnlyList<PosProductItem>> GetAllAsync()
    {
        lock (_lock)
        {
            if (_cache is not null)
                return _cache.AsReadOnly();
        }

        var items = await LoadFromDatabaseAsync();

        lock (_lock)
        {
            _cache = items;
            BuildBarcodeIndex();
            return _cache.AsReadOnly();
        }
    }

    public async Task<PosProductItem?> FindByVariantIdAsync(Guid variantId)
    {
        var all = await GetAllAsync();
        return all.FirstOrDefault(x => x.VariantId == variantId);
    }

    public async Task<IReadOnlyList<PosProductItem>> FindByBarcodeAsync(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode)) return Array.Empty<PosProductItem>();

        await GetAllAsync();

        lock (_lock)
        {
            if (_barcodeIndex is not null && _barcodeIndex.TryGetValue(barcode.Trim(), out var list))
                return list.AsReadOnly();
        }

        return Array.Empty<PosProductItem>();
    }

    public void Invalidate()
    {
        lock (_lock)
        {
            _cache = null;
            _barcodeIndex = null;
        }
    }

    public void UpdateStock(Guid variantId, decimal delta)
    {
        _notifier.NotifyStockChanged(variantId, delta);
    }

    private async Task<List<PosProductItem>> LoadFromDatabaseAsync()
    {
        var variants = await _context.ProductVariants
            .AsNoTracking()
            .Where(v => v.IsActive)
            .Include(v => v.Product)
                .ThenInclude(p => p.Unit)
            .ToListAsync();

        var variantIds = variants.Select(v => v.Id).ToList();
        var productIds = variants.Select(v => v.ProductId).Distinct().ToList();

        var barcodes = await _context.ProductBarcodes
            .AsNoTracking()
            .Where(b => variantIds.Contains(b.VariantId))
            .ToListAsync();

        // Stock = default warehouse of the active branch only.
        var branchId = _activeBranch.ActiveBranchId;
        var warehouseId = await _context.Warehouses
            .AsNoTracking()
            .Where(w => w.BranchId == branchId && w.IsActive)
            .OrderBy(w => w.CreatedAt)
            .ThenBy(w => w.Id)
            .Select(w => (Guid?)w.Id)
            .FirstOrDefaultAsync();

        List<(Guid VariantId, decimal Total)> stocks;
        if (warehouseId is not null)
        {
            var raw = await _context.ProductStocks
                .AsNoTracking()
                .Where(s => variantIds.Contains(s.VariantId) && s.WarehouseId == warehouseId.Value)
                .GroupBy(s => s.VariantId)
                .Select(g => new { VariantId = g.Key, Total = g.Sum(s => s.Quantity.Value) })
                .ToListAsync();
            stocks = raw.Select(x => (x.VariantId, x.Total)).ToList();
        }
        else
        {
            stocks = new List<(Guid, decimal)>();
        }

        var variantCounts = await _context.ProductVariants
            .AsNoTracking()
            .Where(v => productIds.Contains(v.ProductId) && v.IsActive)
            .GroupBy(v => v.ProductId)
            .Select(g => new { ProductId = g.Key, Count = g.Count() })
            .ToListAsync();

        var stockMap = stocks.ToDictionary(s => s.VariantId, s => s.Total);
        var countMap = variantCounts.ToDictionary(c => c.ProductId, c => c.Count);

        var items = new List<PosProductItem>();
        foreach (var v in variants)
        {
            if (v.Product is null || !v.Product.IsActive) continue;

            var barcode = barcodes
                .Where(b => b.VariantId == v.Id)
                .OrderByDescending(b => b.IsPrimary)
                .Select(b => b.Barcode)
                .FirstOrDefault();

            items.Add(new PosProductItem(
                v.Id,
                v.ProductId,
                v.Name,
                v.SKU,
                v.SalePrice.Amount,
                stockMap.TryGetValue(v.Id, out var stock) ? stock : 0m,
                v.IsActive && v.Product.IsActive,
                countMap.TryGetValue(v.ProductId, out var cnt) && cnt > 1,
                v.Product.UnitId,
                v.Product.Unit?.Name,
                v.Product.CategoryId,
                v.Product.ImagePath,
                v.Product.ThumbnailPath,
                barcode));
        }

        return items;
    }

    private void BuildBarcodeIndex()
    {
        if (_cache is null) return;

        var barcodes = _context.ProductBarcodes
            .AsNoTracking()
            .Where(b => _cache.Select(c => c.VariantId).Contains(b.VariantId))
            .ToList();

        var index = new Dictionary<string, List<PosProductItem>>(StringComparer.OrdinalIgnoreCase);
        foreach (var barcode in barcodes)
        {
            var item = _cache.FirstOrDefault(c => c.VariantId == barcode.VariantId);
            if (item is null) continue;
            if (!index.TryGetValue(barcode.Barcode, out var list))
            {
                list = new List<PosProductItem>();
                index[barcode.Barcode] = list;
            }
            if (!list.Contains(item))
                list.Add(item);
        }

        _barcodeIndex = index;
    }

    public void Dispose()
    {
        _notifier.CatalogChanged -= OnCatalogChanged;
        _activeBranch.BranchChanged -= OnBranchChanged;
    }
}

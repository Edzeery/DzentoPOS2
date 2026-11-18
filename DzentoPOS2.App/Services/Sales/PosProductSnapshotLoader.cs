using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DzentoPOS2.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.App.Services.Sales;

/// <summary>
/// Reads one catalog snapshot from the database. Split from
/// <see cref="PosProductStore"/> so the store only owns caching and so tests
/// can slow a load down on purpose.
/// </summary>
public interface IPosProductSnapshotLoader
{
    Task<PosProductSnapshot> LoadAsync(
        AppDbContext context,
        Guid branchId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Loads the sellable variants in a fixed number of set-based queries and
/// builds every in-memory index in one pass:
/// <list type="number">
/// <item>the default warehouse of the branch,</item>
/// <item>the variants joined to their product, unit and category,</item>
/// <item>the stock totals of that warehouse grouped by variant,</item>
/// <item>the barcodes of the active variants joined through the variant.</item>
/// </list>
/// No query passes a list of ids to <c>Contains</c>, and no row is looked up
/// with a per-row <c>FirstOrDefault</c>.
/// </summary>
public sealed class PosProductSnapshotLoader : IPosProductSnapshotLoader
{
    public static PosProductSnapshotLoader Instance { get; } = new();

    public async Task<PosProductSnapshot> LoadAsync(
        AppDbContext context,
        Guid branchId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var total = Stopwatch.StartNew();

        // 1. Default warehouse of the active branch.
        var warehouse = Stopwatch.StartNew();
        Guid? warehouseId = await context.Warehouses
            .AsNoTracking()
            .Where(w => w.BranchId == branchId && w.IsActive)
            .OrderBy(w => w.CreatedAt)
            .ThenBy(w => w.Id)
            .Select(w => (Guid?)w.Id)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        warehouse.Stop();

        cancellationToken.ThrowIfCancellationRequested();

        // 2. Variants with everything the card needs, in one join.
        var variants = Stopwatch.StartNew();
        List<VariantRow> variantRows = await context.ProductVariants
            .AsNoTracking()
            .Where(v => v.IsActive && v.Product.IsActive)
            .OrderBy(v => v.Product.Name)
            .ThenBy(v => v.Name)
            .Select(v => new VariantRow(
                v.Id,
                v.ProductId,
                v.Name,
                v.SKU,
                v.Product.SKU,
                v.SalePrice.Amount,
                v.Product.UnitId,
                v.Product.Unit.Name,
                v.Product.CategoryId,
                v.Product.Category.Name,
                v.Product.ImagePath,
                v.Product.ThumbnailPath))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        variants.Stop();

        cancellationToken.ThrowIfCancellationRequested();

        // 3. Stock of the default warehouse, grouped in SQL.
        var stocksWatch = Stopwatch.StartNew();
        Dictionary<Guid, decimal> stockByVariant = new();
        if (warehouseId is not null)
        {
            var raw = await context.ProductStocks
                .AsNoTracking()
                .Where(s => s.WarehouseId == warehouseId.Value
                            && s.Variant.IsActive
                            && s.Variant.Product.IsActive)
                .GroupBy(s => s.VariantId)
                .Select(g => new { VariantId = g.Key, Total = g.Sum(s => s.Quantity.Value) })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            stockByVariant = new Dictionary<Guid, decimal>(raw.Count);
            foreach (var row in raw)
                stockByVariant[row.VariantId] = row.Total;
        }
        stocksWatch.Stop();

        cancellationToken.ThrowIfCancellationRequested();

        // 4. Barcodes, joined through the variant so no id list is sent.
        var barcodesWatch = Stopwatch.StartNew();
        List<BarcodeRow> barcodeRows = await context.ProductBarcodes
            .AsNoTracking()
            .Where(b => b.Variant.IsActive && b.Variant.Product.IsActive)
            .Select(b => new BarcodeRow(b.VariantId, b.Barcode, b.IsPrimary))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        barcodesWatch.Stop();

        cancellationToken.ThrowIfCancellationRequested();

        // Group the barcode rows once: the same pass feeds the barcode count,
        // the primary barcode and the barcode index.
        var projection = Stopwatch.StartNew();
        var barcodesByVariant = new Dictionary<Guid, List<BarcodeRow>>(barcodeRows.Count);
        foreach (BarcodeRow row in barcodeRows)
        {
            if (!barcodesByVariant.TryGetValue(row.VariantId, out List<BarcodeRow>? list))
            {
                list = new List<BarcodeRow>(1);
                barcodesByVariant[row.VariantId] = list;
            }

            list.Add(row);
        }

        // A variant has variants when its product has more than one active one.
        var activeVariantsPerProduct = new Dictionary<Guid, int>(variantRows.Count);
        foreach (VariantRow row in variantRows)
        {
            activeVariantsPerProduct.TryGetValue(row.ProductId, out int count);
            activeVariantsPerProduct[row.ProductId] = count + 1;
        }

        var items = new List<PosProductItem>(variantRows.Count);
        var indexByVariant = new Dictionary<Guid, int>(variantRows.Count);

        foreach (VariantRow row in variantRows)
        {
            int barcodeCount = 0;
            string? primaryBarcode = null;
            if (barcodesByVariant.TryGetValue(row.VariantId, out List<BarcodeRow>? variantBarcodes))
            {
                barcodeCount = variantBarcodes.Count;
                primaryBarcode = PickPrimaryBarcode(variantBarcodes);
            }

            items.Add(new PosProductItem(
                VariantId: row.VariantId,
                ProductId: row.ProductId,
                Name: row.VariantName ?? string.Empty,
                SKU: row.Sku ?? row.ProductSku,
                SalePrice: row.SalePrice,
                Stock: stockByVariant.TryGetValue(row.VariantId, out decimal stock) ? stock : 0m,
                IsActive: true,
                HasVariants: activeVariantsPerProduct[row.ProductId] > 1,
                UnitId: row.UnitId,
                UnitName: row.UnitName,
                CategoryId: row.CategoryId,
                ImagePath: row.ImagePath,
                ThumbnailPath: row.ThumbnailPath,
                PrimaryBarcode: primaryBarcode,
                BarcodeCount: barcodeCount,
                CategoryName: row.CategoryName));

            indexByVariant[row.VariantId] = items.Count - 1;
        }
        projection.Stop();

        // The barcode index reuses the grouping built for the counts, so no
        // barcode is read or looked up twice.
        var indexWatch = Stopwatch.StartNew();
        var indexByBarcode = new Dictionary<string, List<int>>(barcodeRows.Count, StringComparer.OrdinalIgnoreCase);
        foreach (BarcodeRow barcode in barcodeRows)
        {
            if (!indexByVariant.TryGetValue(barcode.VariantId, out int itemIndex))
                continue;

            if (!indexByBarcode.TryGetValue(barcode.Barcode, out List<int>? indices))
            {
                indices = new List<int>(1);
                indexByBarcode[barcode.Barcode] = indices;
            }

            if (!indices.Contains(itemIndex))
                indices.Add(itemIndex);
        }
        indexWatch.Stop();

        total.Stop();

        return new PosProductSnapshot(
            items,
            indexByVariant,
            indexByBarcode,
            new PosLoadTimings(
                warehouse.Elapsed.TotalMilliseconds,
                variants.Elapsed.TotalMilliseconds,
                stocksWatch.Elapsed.TotalMilliseconds,
                barcodesWatch.Elapsed.TotalMilliseconds,
                projection.Elapsed.TotalMilliseconds,
                indexWatch.Elapsed.TotalMilliseconds,
                total.Elapsed.TotalMilliseconds));
    }

    /// <summary>The primary barcode, or the first one by ordinal order.</summary>
    private static string? PickPrimaryBarcode(List<BarcodeRow> barcodes)
    {
        string? fallback = null;

        foreach (BarcodeRow row in barcodes)
        {
            if (row.IsPrimary)
                return row.Barcode;

            if (fallback is null || string.CompareOrdinal(row.Barcode, fallback) < 0)
                fallback = row.Barcode;
        }

        return fallback;
    }

    private sealed record VariantRow(
        Guid VariantId,
        Guid ProductId,
        string? VariantName,
        string? Sku,
        string? ProductSku,
        decimal SalePrice,
        Guid UnitId,
        string? UnitName,
        Guid CategoryId,
        string? CategoryName,
        string? ImagePath,
        string? ThumbnailPath);

    private readonly record struct BarcodeRow(Guid VariantId, string Barcode, bool IsPrimary);
}

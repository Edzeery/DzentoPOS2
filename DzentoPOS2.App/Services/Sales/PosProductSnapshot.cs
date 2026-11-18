using System;
using System.Collections.Generic;

namespace DzentoPOS2.App.Services.Sales;

/// <summary>
/// Where the time of one catalog load went. The POS diagnostics report these
/// numbers so a regression can be traced to a phase instead of a total.
/// </summary>
public sealed record PosLoadTimings(
    double WarehouseQueryMs,
    double VariantQueryMs,
    double StockQueryMs,
    double BarcodeQueryMs,
    double ProjectionMs,
    double IndexMs,
    double TotalMs)
{
    public static PosLoadTimings Zero { get; } = new(0, 0, 0, 0, 0, 0, 0);
}

/// <summary>
/// One immutable catalog load: the sellable variants, a variant index and a
/// barcode index that all share the same item instances. A stock delta is the
/// only mutation and it happens under the store's lock.
/// </summary>
public sealed class PosProductSnapshot
{
    private readonly List<PosProductItem> _items;
    private readonly Dictionary<Guid, int> _indexByVariant;
    private readonly Dictionary<string, List<int>> _indexByBarcode;

    internal PosProductSnapshot(
        List<PosProductItem> items,
        Dictionary<Guid, int> indexByVariant,
        Dictionary<string, List<int>> indexByBarcode,
        PosLoadTimings timings)
    {
        _items = items;
        _indexByVariant = indexByVariant;
        _indexByBarcode = indexByBarcode;
        Timings = timings;
    }

    public static PosProductSnapshot Empty { get; } = new(
        new List<PosProductItem>(),
        new Dictionary<Guid, int>(),
        new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase),
        PosLoadTimings.Zero);

    /// <summary>The variants in display order. Read-only for callers.</summary>
    public IReadOnlyList<PosProductItem> Items => _items;

    public PosLoadTimings Timings { get; }

    public int Count => _items.Count;

    /// <summary>O(1) lookup used by the cart and the sale service.</summary>
    public PosProductItem? FindByVariantId(Guid variantId) =>
        _indexByVariant.TryGetValue(variantId, out int index) ? _items[index] : null;

    /// <summary>
    /// Every variant that carries this barcode. The index is case-insensitive
    /// because barcode scanners and manual typing are not consistent.
    /// </summary>
    public IReadOnlyList<PosProductItem> FindByBarcode(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return Array.Empty<PosProductItem>();

        if (!_indexByBarcode.TryGetValue(barcode.Trim(), out List<int>? indices) || indices.Count == 0)
            return Array.Empty<PosProductItem>();

        var matches = new PosProductItem[indices.Count];
        for (int i = 0; i < indices.Count; i++)
            matches[i] = _items[indices[i]];

        return matches;
    }

    /// <summary>
    /// Applies an optimistic stock change to the cached instance. Returns false
    /// when the variant is not part of this snapshot, which is the case for a
    /// delta that arrives before the first load completed.
    /// </summary>
    public bool ApplyStockDelta(Guid variantId, decimal delta)
    {
        if (delta == 0) return false;
        if (!_indexByVariant.TryGetValue(variantId, out int index)) return false;

        PosProductItem item = _items[index];
        _items[index] = item with { Stock = item.Stock + delta };
        return true;
    }
}

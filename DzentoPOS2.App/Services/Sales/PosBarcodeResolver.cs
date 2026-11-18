namespace DzentoPOS2.App.Services.Sales;

/// <summary>
/// Resolves a scanned or typed barcode to the sellable variants behind it.
/// This is the only barcode entry point used by the POS screen, so the camera,
/// a keyboard-wedge reader and manual typing all take the same path.
/// </summary>
public interface IPosBarcodeResolver
{
    Task<IReadOnlyList<PosProductItem>> ResolveAsync(
        string barcode,
        CancellationToken cancellationToken = default);
}

public sealed class PosBarcodeResolver : IPosBarcodeResolver
{
    private readonly IPosProductStore _store;

    public PosBarcodeResolver(IPosProductStore store)
    {
        _store = store;
    }

    public async Task<IReadOnlyList<PosProductItem>> ResolveAsync(
        string barcode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return Array.Empty<PosProductItem>();

        var matches = await _store.FindByBarcodeAsync(barcode.Trim());
        if (matches.Count == 0)
            return matches;

        // A shared barcode can point at several variants; collapse duplicates
        // so the picker shows one row per variant.
        return matches
            .GroupBy(m => m.VariantId)
            .Select(g => g.First())
            .OrderByDescending(m => m.IsActive)
            .ThenBy(m => m.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}

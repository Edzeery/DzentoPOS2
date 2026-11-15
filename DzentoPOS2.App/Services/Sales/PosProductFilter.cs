using System;
using System.Collections.Generic;
using System.Linq;

namespace DzentoPOS2.App.Services.Sales;

/// <summary>
/// Pure filtering for the POS catalog. The screen keeps the full catalog in
/// memory and this helper narrows it down by search term (name, SKU or
/// barcode) and by category. It never mutates the source list and returns the
/// same instance when nothing is filtered.
/// </summary>
public static class PosProductFilter
{
    /// <summary>
    /// Returns the items that match both the search term and the category.
    /// </summary>
    /// <param name="items">Full catalog.</param>
    /// <param name="term">Search text. Null, empty or whitespace matches everything.</param>
    /// <param name="categoryId">Category to keep. Null or <see cref="Guid.Empty"/> means all.</param>
    /// <param name="barcodeProductIds">
    /// Extra product ids resolved from barcodes that match <paramref name="term"/>.
    /// Products are matched by id, so a barcode shared by several variants keeps
    /// every variant of that product.
    /// </param>
    public static IReadOnlyList<PosProductItem> Apply(
        IReadOnlyList<PosProductItem> items,
        string? term,
        Guid? categoryId = null,
        IReadOnlyCollection<Guid>? barcodeProductIds = null)
    {
        ArgumentNullException.ThrowIfNull(items);

        string? needle = Normalize(term);
        bool hasTerm = needle is not null;
        bool hasCategory = categoryId.HasValue && categoryId.Value != Guid.Empty;

        if (!hasTerm && !hasCategory)
            return items;

        var result = new List<PosProductItem>(items.Count);
        foreach (PosProductItem item in items)
        {
            if (hasCategory && item.CategoryId != categoryId!.Value)
                continue;

            if (hasTerm && !Matches(item, needle!, barcodeProductIds))
                continue;

            result.Add(item);
        }

        return result;
    }

    /// <summary>True when one sellable variant matches the search term.</summary>
    public static bool Matches(
        PosProductItem item,
        string? term,
        IReadOnlyCollection<Guid>? barcodeProductIds = null)
    {
        ArgumentNullException.ThrowIfNull(item);

        string? needle = Normalize(term);
        if (needle is null)
            return true;

        if (Contains(item.Name, needle))
            return true;

        if (Contains(item.SKU, needle))
            return true;

        if (Contains(item.PrimaryBarcode, needle))
            return true;

        return barcodeProductIds is not null && barcodeProductIds.Contains(item.ProductId);
    }

    /// <summary>
    /// Trims the search text and reports it as absent when it carries no
    /// characters to look for.
    /// </summary>
    public static string? Normalize(string? term)
    {
        string? trimmed = term?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    private static bool Contains(string? value, string needle) =>
        !string.IsNullOrEmpty(value)
        && value.Contains(needle, StringComparison.OrdinalIgnoreCase);
}

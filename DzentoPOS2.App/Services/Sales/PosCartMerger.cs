using DzentoPOS2.Shared.Contracts.DTOs.Sales;

namespace DzentoPOS2.App.Services.Sales;

/// <summary>
/// Adds a cart line, merging it into an existing line when the product,
/// variant, batch and serial unit all match. Two barcodes that belong to the
/// same variant therefore produce a single line instead of duplicates.
/// </summary>
public static class PosCartMerger
{
    public static CreateSaleItemDto AddOrMerge(
        IList<CreateSaleItemDto> cart,
        CreateSaleItemDto line)
    {
        ArgumentNullException.ThrowIfNull(cart);
        ArgumentNullException.ThrowIfNull(line);

        var existing = cart.FirstOrDefault(i =>
            i.ProductId == line.ProductId &&
            i.VariantId == line.VariantId &&
            i.BatchId == line.BatchId &&
            i.SerialUnitId == line.SerialUnitId);

        if (existing is null)
        {
            cart.Add(line);
            return line;
        }

        existing.Quantity += line.Quantity;
        return existing;
    }
}

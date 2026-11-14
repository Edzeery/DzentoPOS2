using DzentoPOS2.Shared.Contracts.DTOs.Sales;

namespace DzentoPOS2.App.Services.Sales;

/// <summary>
/// Copies the POS cart into the checkout DTOs. The cart already holds
/// <see cref="CreateSaleItemDto"/> items, so this looks redundant, but it is the
/// single place that guarantees the variant, batch and serial selections reach
/// <see cref="ISaleService"/>. A regression here used to record every line
/// against the product's default variant.
/// </summary>
public static class PosCheckoutMapper
{
    public static List<CreateSaleItemDto> ToSaleItems(IEnumerable<CreateSaleItemDto> cart)
    {
        ArgumentNullException.ThrowIfNull(cart);

        return cart.Select(Clone).ToList();
    }

    private static CreateSaleItemDto Clone(CreateSaleItemDto line) => new()
    {
        ProductId = line.ProductId,
        VariantId = line.VariantId,
        SerialUnitId = line.SerialUnitId,
        BatchId = line.BatchId,
        VariantName = line.VariantName,
        Quantity = line.Quantity,
        Price = line.Price,
        Description = line.Description
    };
}

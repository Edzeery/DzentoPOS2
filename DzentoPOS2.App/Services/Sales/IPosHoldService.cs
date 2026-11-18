using DzentoPOS2.Shared.Contracts.DTOs.Sales;

namespace DzentoPOS2.App.Services.Sales;

/// <summary>
/// App-level (not form-level) store for held POS carts. A hold survives a
/// form close so the cashier can recall it later; the shell warns on
/// application or shift close when any hold exists.
/// </summary>
public interface IPosHoldService
{
    int Count { get; }
    void Hold(IReadOnlyList<CreateSaleItemDto> items, Guid customerId);
    IReadOnlyList<CreateSaleItemDto> Recall(int index);
    void Clear();
    event EventHandler? Changed;
}

public sealed class PosHoldService : IPosHoldService
{
    private readonly List<List<CreateSaleItemDto>> _holds = new();

    public int Count => _holds.Count;

    public event EventHandler? Changed;

    public void Hold(IReadOnlyList<CreateSaleItemDto> items, Guid customerId)
    {
        var copy = items.Select(i => new CreateSaleItemDto
        {
            ProductId = i.ProductId,
            VariantId = i.VariantId,
            VariantName = i.VariantName,
            Quantity = i.Quantity,
            Price = i.Price,
            OriginalPrice = i.OriginalPrice,
            PriceOverridden = i.PriceOverridden,
            Description = i.Description
        }).ToList();
        _holds.Add(copy);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public IReadOnlyList<CreateSaleItemDto> Recall(int index)
    {
        if (index < 0 || index >= _holds.Count) return Array.Empty<CreateSaleItemDto>();
        var hold = _holds[index];
        _holds.RemoveAt(index);
        Changed?.Invoke(this, EventArgs.Empty);
        return hold;
    }

    public void Clear() { _holds.Clear(); Changed?.Invoke(this, EventArgs.Empty); }
}

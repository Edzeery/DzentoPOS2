using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DzentoPOS2.App.Services.Sales;

/// <summary>
/// Notifies subscribers when the catalog (products, variants, barcodes) changes
/// so the POS cache can be invalidated.
/// </summary>
public interface ICatalogChangeNotifier
{
    event EventHandler<CatalogChangedEventArgs>? CatalogChanged;

    void NotifyProductChanged(Guid productId);
    void NotifyVariantChanged(Guid variantId);
    void NotifyBarcodeChanged(string barcode);
    void NotifyStockChanged(Guid variantId, decimal delta);
}

public class CatalogChangedEventArgs : EventArgs
{
    public Guid? ProductId { get; init; }
    public Guid? VariantId { get; init; }
    public string? Barcode { get; init; }
    public decimal StockDelta { get; init; }
    public bool FullInvalidation { get; init; }
}

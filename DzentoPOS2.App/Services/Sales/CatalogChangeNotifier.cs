using System;
using System.Collections.Generic;

namespace DzentoPOS2.App.Services.Sales;

public class CatalogChangeNotifier : ICatalogChangeNotifier
{
    public event EventHandler<CatalogChangedEventArgs>? CatalogChanged;

    public void NotifyProductChanged(Guid productId)
    {
        CatalogChanged?.Invoke(this, new CatalogChangedEventArgs
        {
            ProductId = productId,
            FullInvalidation = true
        });
    }

    public void NotifyVariantChanged(Guid variantId)
    {
        CatalogChanged?.Invoke(this, new CatalogChangedEventArgs
        {
            VariantId = variantId,
            FullInvalidation = true
        });
    }

    public void NotifyBarcodeChanged(string barcode)
    {
        CatalogChanged?.Invoke(this, new CatalogChangedEventArgs
        {
            Barcode = barcode,
            FullInvalidation = true
        });
    }

    public void NotifyStockChanged(Guid variantId, decimal delta)
    {
        CatalogChanged?.Invoke(this, new CatalogChangedEventArgs
        {
            VariantId = variantId,
            StockDelta = delta,
            FullInvalidation = false
        });
    }
}

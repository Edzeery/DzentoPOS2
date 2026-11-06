using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DzentoPOS2.App.Services.Inventory;

public interface IBarcodeService
{
    Task<Guid> GetDefaultVariantIdAsync(Guid productId);
    Task<List<string>> GetBarcodesAsync(Guid variantId);
    Task<bool> IsUniqueAsync(string barcode, Guid? excludeId = null);
    Task AddAsync(Guid variantId, string barcode, bool isPrimary);
    Task RemoveAsync(Guid barcodeId);
    Task RemoveByValueAsync(Guid variantId, string barcode);
    string GenerateInternal();
}
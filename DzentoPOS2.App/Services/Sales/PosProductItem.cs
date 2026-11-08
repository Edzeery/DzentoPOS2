using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DzentoPOS2.Core.Entities.Inventory;

namespace DzentoPOS2.App.Services.Sales;

/// <summary>
/// Lightweight projection of a sellable variant for the POS screen. Replaces
/// the old full-entity load and the (never-implemented) PosProductItem cache.
/// </summary>
public record PosProductItem(
    Guid VariantId,
    Guid ProductId,
    string Name,
    string? SKU,
    decimal SalePrice,
    decimal Stock,
    bool IsActive,
    bool HasVariants,
    Guid UnitId,
    string? UnitName,
    Guid CategoryId,
    string? ImagePath,
    string? ThumbnailPath,
    string? PrimaryBarcode);

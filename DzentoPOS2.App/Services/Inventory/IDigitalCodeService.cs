using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Core.Enums;
using DzentoPOS2.Shared.Contracts.DTOs.Inventory;

namespace DzentoPOS2.App.Services.Inventory;

/// <summary>
/// Pool of digital codes (prepaid cards / activation keys): bulk import,
/// listing/search for the admin screen, allocation when a digital-kind product
/// is sold and release back to the pool when that sale is voided.
/// </summary>
public interface IDigitalCodeService
{
    /// <summary>Every variant of a digital-kind product (for the selector).</summary>
    Task<List<DigitalVariantOptionDto>> GetDigitalVariantsAsync(CancellationToken ct = default);

    /// <summary>Codes filtered by variant, status and (optionally) a code fragment.</summary>
    Task<List<DigitalCodeDto>> SearchAsync(
        Guid? variantId,
        DigitalCodeStatus? status,
        string? term,
        CancellationToken ct = default);

    /// <summary>Adds codes to a variant, skipping blanks and duplicates already present.</summary>
    Task<DigitalCodeImportResultDto> ImportAsync(
        Guid variantId,
        IEnumerable<string> codes,
        CancellationToken ct = default);

    /// <summary>Number of <see cref="DigitalCodeStatus.Available"/> codes of a variant.</summary>
    Task<int> GetAvailableCountAsync(Guid variantId, CancellationToken ct = default);

    /// <summary>
    /// Marks up to <paramref name="quantity"/> available codes as delivered to
    /// the given invoice line and returns them. Called inside the sale
    /// transaction; saves its changes so the caller sees the links.
    /// </summary>
    Task<List<DigitalCode>> AllocateForSaleAsync(
        Guid variantId,
        Guid salesInvoiceId,
        Guid salesInvoiceItemId,
        int quantity,
        DateTime deliveredAt,
        CancellationToken ct = default);

    /// <summary>
    /// Releases every code delivered by the given invoice back to the pool
    /// (used when the invoice is voided). Returns how many codes were released.
    /// </summary>
    Task<int> ReturnForSaleAsync(Guid salesInvoiceId, CancellationToken ct = default);

    /// <summary>Removes a code from circulation (soft delete).</summary>
    Task CancelAsync(Guid id, CancellationToken ct = default);
}

using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Core.Enums;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Shared.Contracts.DTOs.Inventory;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.App.Services.Inventory;

/// <inheritdoc cref="IDigitalCodeService" />
public class DigitalCodeService : IDigitalCodeService
{
    private readonly AppDbContext _context;

    public DigitalCodeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<DigitalVariantOptionDto>> GetDigitalVariantsAsync(CancellationToken ct = default)
    {
        var variants = await _context.ProductVariants
            .AsNoTracking()
            .Include(v => v.Product)
            .Where(v => v.Product.ProductKind == ProductKind.DigitalCode && v.IsActive)
            .OrderBy(v => v.Product.Name)
            .ThenBy(v => v.Name)
            .ToListAsync(ct);

        return variants
            .Select(v => new DigitalVariantOptionDto
            {
                VariantId = v.Id,
                ProductId = v.ProductId,
                DisplayName = DisplayName(v.Product?.Name, v.Name, v.IsDefault)
            })
            .ToList();
    }

    public async Task<List<DigitalCodeDto>> SearchAsync(
        Guid? variantId,
        DigitalCodeStatus? status,
        string? term,
        CancellationToken ct = default)
    {
        var query = _context.DigitalCodes
            .AsNoTracking()
            .Include(c => c.Variant)
                .ThenInclude(v => v.Product)
            .AsQueryable();

        if (variantId.HasValue && variantId.Value != Guid.Empty)
            query = query.Where(c => c.VariantId == variantId.Value);

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(term))
        {
            var trimmed = term.Trim();
            query = query.Where(c => c.Code.Contains(trimmed));
        }

        var codes = await query
            .OrderByDescending(c => c.CreatedAt)
            .Take(2000)
            .ToListAsync(ct);

        var invoiceIds = codes
            .Where(c => c.SalesInvoiceId.HasValue)
            .Select(c => c.SalesInvoiceId!.Value)
            .Distinct()
            .ToList();

        var invoiceNumbers = invoiceIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _context.SalesInvoices
                .AsNoTracking()
                .Where(i => invoiceIds.Contains(i.Id))
                .ToDictionaryAsync(i => i.Id, i => i.InvoiceNumber.Value, ct);

        return codes.Select(c => new DigitalCodeDto
        {
            Id = c.Id,
            Code = c.Code,
            Pin = c.Pin,
            VariantId = c.VariantId,
            VariantName = DisplayName(c.Variant?.Product?.Name, c.Variant?.Name, c.Variant?.IsDefault ?? false),
            Status = c.Status,
            DeliveredAt = c.DeliveredAt,
            SalesInvoiceId = c.SalesInvoiceId,
            InvoiceNumber = c.SalesInvoiceId.HasValue && invoiceNumbers.TryGetValue(c.SalesInvoiceId.Value, out var number)
                ? number
                : null,
            CreatedAt = c.CreatedAt
        }).ToList();
    }

    public async Task<DigitalCodeImportResultDto> ImportAsync(
        Guid variantId,
        IEnumerable<string> codes,
        CancellationToken ct = default)
    {
        if (variantId == Guid.Empty)
            throw new InvalidOperationException("A variant is required to import digital codes.");

        // Materialise the raw lines so the skipped count can report every line
        // that was not imported: blanks, repeated lines and codes already on
        // file for this variant.
        var lines = codes
            .Select(c => (c ?? string.Empty).Trim())
            .ToList();

        var incoming = lines
            .Where(c => c.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (incoming.Count == 0)
            return new DigitalCodeImportResultDto { Imported = 0, Skipped = lines.Count };

        var existing = await _context.DigitalCodes
            .IgnoreQueryFilters()
            .Where(c => c.VariantId == variantId && incoming.Contains(c.Code))
            .Select(c => c.Code)
            .ToListAsync(ct);

        var existingSet = new HashSet<string>(existing, StringComparer.Ordinal);

        var toAdd = incoming
            .Where(code => !existingSet.Contains(code))
            .Select(code => new DigitalCode
            {
                VariantId = variantId,
                Code = code,
                Status = DigitalCodeStatus.Available
            })
            .ToList();

        if (toAdd.Count > 0)
        {
            await _context.DigitalCodes.AddRangeAsync(toAdd, ct);
            await _context.SaveChangesAsync(ct);
        }

        return new DigitalCodeImportResultDto
        {
            Imported = toAdd.Count,
            Skipped = lines.Count - toAdd.Count
        };
    }

    public Task<int> GetAvailableCountAsync(Guid variantId, CancellationToken ct = default) =>
        _context.DigitalCodes
            .AsNoTracking()
            .CountAsync(c => c.VariantId == variantId && c.Status == DigitalCodeStatus.Available, ct);

    public async Task<List<DigitalCode>> AllocateForSaleAsync(
        Guid variantId,
        Guid salesInvoiceId,
        Guid salesInvoiceItemId,
        int quantity,
        DateTime deliveredAt,
        CancellationToken ct = default)
    {
        if (quantity <= 0)
            return new List<DigitalCode>();

        var available = await _context.DigitalCodes
            .Where(c => c.VariantId == variantId && c.Status == DigitalCodeStatus.Available)
            .OrderBy(c => c.CreatedAt)
            .Take(quantity)
            .ToListAsync(ct);

        foreach (var code in available)
        {
            code.Status = DigitalCodeStatus.Delivered;
            code.SalesInvoiceId = salesInvoiceId;
            code.SalesInvoiceItemId = salesInvoiceItemId;
            code.DeliveredAt = deliveredAt;
            code.UpdatedAt = deliveredAt;
        }

        if (available.Count > 0)
            await _context.SaveChangesAsync(ct);

        return available;
    }

    public async Task<int> ReturnForSaleAsync(Guid salesInvoiceId, CancellationToken ct = default)
    {
        var delivered = await _context.DigitalCodes
            .Where(c => c.SalesInvoiceId == salesInvoiceId && c.Status == DigitalCodeStatus.Delivered)
            .ToListAsync(ct);

        foreach (var code in delivered)
        {
            code.Status = DigitalCodeStatus.Available;
            code.SalesInvoiceId = null;
            code.SalesInvoiceItemId = null;
            code.DeliveredAt = null;
            code.UpdatedAt = DateTime.UtcNow;
        }

        if (delivered.Count > 0)
            await _context.SaveChangesAsync(ct);

        return delivered.Count;
    }

    public async Task CancelAsync(Guid id, CancellationToken ct = default)
    {
        var code = await _context.DigitalCodes.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (code == null)
            return;

        code.Status = DigitalCodeStatus.Cancelled;
        code.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
    }

    private static string DisplayName(string? productName, string? variantName, bool isDefault)
    {
        var product = string.IsNullOrWhiteSpace(productName) ? string.Empty : productName!;

        if (string.IsNullOrWhiteSpace(variantName) || isDefault || string.Equals(product, variantName, StringComparison.Ordinal))
            return product;

        return $"{product} - {variantName}";
    }
}

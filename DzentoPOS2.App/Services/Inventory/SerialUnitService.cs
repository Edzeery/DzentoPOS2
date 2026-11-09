using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Core.Enums;
using DzentoPOS2.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.App.Services.Inventory;

public interface ISerialUnitService
{
    Task<List<SerialUnit>> GetAvailableUnitsAsync(Guid variantId, Guid warehouseId, CancellationToken ct = default);
    Task<SerialUnit?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<SerialUnit?> GetBySerialOrImeiAsync(string serialOrImei, CancellationToken ct = default);
    Task<WarrantyInfoDto?> GetWarrantyInfoAsync(string serialOrImei, CancellationToken ct = default);
    Task CreateAsync(SerialUnit unit, CancellationToken ct = default);
    Task MarkSoldAsync(Guid serialUnitId, Guid salesInvoiceItemId, DateTime saleDate, CancellationToken ct = default);
    Task MarkReturnedAsync(Guid serialUnitId, CancellationToken ct = default);
}

public class WarrantyInfoDto
{
    public string? SerialNumber { get; set; }
    public string? IMEI { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public DateTime? SaleDate { get; set; }
    public DateTime? WarrantyExpiry { get; set; }
    public SerialUnitStatus Status { get; set; }
    public bool IsUnderWarranty => WarrantyExpiry.HasValue && WarrantyExpiry.Value >= DateTime.UtcNow;
}

public class SerialUnitService : ISerialUnitService
{
    private readonly AppDbContext _context;

    public SerialUnitService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<SerialUnit>> GetAvailableUnitsAsync(Guid variantId, Guid warehouseId, CancellationToken ct = default)
    {
        return await _context.SerialUnits
            .AsNoTracking()
            .Where(s => s.VariantId == variantId && s.WarehouseId == warehouseId && s.Status == SerialUnitStatus.Available)
            .ToListAsync(ct);
    }

    public async Task<SerialUnit?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.SerialUnits
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<SerialUnit?> GetBySerialOrImeiAsync(string serialOrImei, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(serialOrImei)) return null;
        var trimmed = serialOrImei.Trim();
        return await _context.SerialUnits
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SerialNumber == trimmed || s.Imei1 == trimmed || s.Imei2 == trimmed, ct);
    }

    public async Task<WarrantyInfoDto?> GetWarrantyInfoAsync(string serialOrImei, CancellationToken ct = default)
    {
        var unit = await GetBySerialOrImeiAsync(serialOrImei, ct);
        if (unit == null) return null;

        var variant = await _context.ProductVariants
            .AsNoTracking()
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Id == unit.VariantId, ct);

        return new WarrantyInfoDto
        {
            SerialNumber = unit.SerialNumber,
            IMEI = unit.Imei1,
            ProductName = variant?.Product?.Name ?? "",
            VariantName = variant?.Name ?? "",
            SaleDate = unit.SoldAt,
            WarrantyExpiry = unit.WarrantyExpiresAt,
            Status = unit.Status
        };
    }

    public async Task CreateAsync(SerialUnit unit, CancellationToken ct = default)
    {
        await _context.SerialUnits.AddAsync(unit, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task MarkSoldAsync(Guid serialUnitId, Guid salesInvoiceItemId, DateTime saleDate, CancellationToken ct = default)
    {
        var unit = await _context.SerialUnits.FirstOrDefaultAsync(s => s.Id == serialUnitId, ct);
        if (unit == null) return;

        unit.Status = SerialUnitStatus.Sold;
        unit.SalesInvoiceItemId = salesInvoiceItemId;
        unit.SoldAt = saleDate;
        unit.WarrantyStartsAt = saleDate;

        var variant = await _context.ProductVariants
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == unit.VariantId, ct);
        if (variant?.WarrantyMonths.HasValue == true && variant.WarrantyMonths.Value > 0)
        {
            unit.WarrantyExpiresAt = saleDate.AddMonths(variant.WarrantyMonths.Value);
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task MarkReturnedAsync(Guid serialUnitId, CancellationToken ct = default)
    {
        var unit = await _context.SerialUnits.FirstOrDefaultAsync(s => s.Id == serialUnitId, ct);
        if (unit == null) return;

        unit.Status = SerialUnitStatus.Returned;
        unit.SalesInvoiceItemId = null;
        unit.SoldAt = null;
        unit.WarrantyStartsAt = null;
        unit.WarrantyExpiresAt = null;

        await _context.SaveChangesAsync(ct);
    }
}

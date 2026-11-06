using DzentoPOS2.App.Services.Sales;
using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.App.Services.Inventory;

public class BarcodeService : IBarcodeService
{
    private readonly AppDbContext _context;
    private readonly ICatalogChangeNotifier _notifier;
    private static int _counter = 1000;

    public BarcodeService(AppDbContext context, ICatalogChangeNotifier notifier)
    {
        _context = context;
        _notifier = notifier;
    }

    public async Task<Guid> GetDefaultVariantIdAsync(Guid productId)
    {
        var v = await _context.ProductVariants
            .AsNoTracking()
            .Where(x => x.ProductId == productId && x.IsDefault)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();
        if (v == Guid.Empty)
        {
            // create default variant on the fly
            var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId);
            if (product is null) return Guid.Empty;
            var variant = new ProductVariant
            {
                ProductId = productId,
                Name = product.Name,
                CostPrice = product.CostPrice,
                SalePrice = product.SalePrice,
                MinimumStock = product.MinimumStock,
                TrackStock = product.TrackStock,
                IsActive = product.IsActive,
                IsDefault = true
            };
            await _context.ProductVariants.AddAsync(variant);
            await _context.SaveChangesAsync();
            return variant.Id;
        }
        return v;
    }

    public async Task<List<string>> GetBarcodesAsync(Guid variantId)
    {
        return await _context.ProductBarcodes
            .AsNoTracking()
            .Where(b => b.VariantId == variantId)
            .OrderByDescending(b => b.IsPrimary)
            .Select(b => b.Barcode)
            .ToListAsync();
    }

    public async Task<bool> IsUniqueAsync(string barcode, Guid? excludeId = null)
    {
        var q = _context.ProductBarcodes.AsNoTracking().Where(b => b.Barcode == barcode);
        if (excludeId.HasValue) q = q.Where(b => b.Id != excludeId.Value);
        return !await q.AnyAsync();
    }

    public async Task AddAsync(Guid variantId, string barcode, bool isPrimary)
    {
        if (string.IsNullOrWhiteSpace(barcode)) return;
        var exists = await _context.ProductBarcodes.AnyAsync(b => b.VariantId == variantId && b.Barcode == barcode);
        if (exists) return;
        await _context.ProductBarcodes.AddAsync(new ProductBarcode
        {
            VariantId = variantId,
            Barcode = barcode.Trim(),
            IsPrimary = isPrimary
        });
        await _context.SaveChangesAsync();

        _notifier.NotifyBarcodeChanged(barcode.Trim());
    }

    public async Task RemoveByValueAsync(Guid variantId, string barcode)
    {
        var b = await _context.ProductBarcodes.FirstOrDefaultAsync(x => x.VariantId == variantId && x.Barcode == barcode);
        if (b != null)
        {
            _context.ProductBarcodes.Remove(b);
            await _context.SaveChangesAsync();

            _notifier.NotifyBarcodeChanged(barcode.Trim());
        }
    }

    public async Task RemoveAsync(Guid barcodeId)
    {
        var b = await _context.ProductBarcodes.FirstOrDefaultAsync(x => x.Id == barcodeId);
        if (b != null)
        {
            _context.ProductBarcodes.Remove(b);
            await _context.SaveChangesAsync();

            _notifier.NotifyBarcodeChanged(b.Barcode);
        }
    }

    public string GenerateInternal()
    {
        return "INT-" + Interlocked.Increment(ref _counter).ToString("D6");
    }

    public async Task<Guid?> FindVariantIdByBarcodeAsync(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode)) return null;
        var b = await _context.ProductBarcodes
            .AsNoTracking()
            .Where(x => x.Barcode == barcode.Trim())
            .OrderByDescending(x => x.IsPrimary)
            .Select(x => x.VariantId)
            .FirstOrDefaultAsync();
        return b == Guid.Empty ? null : b;
    }
}
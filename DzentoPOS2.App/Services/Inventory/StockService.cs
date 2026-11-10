using DzentoPOS2.App.Authorization;
using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.Core.Constants;
using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Core.Enums;
using DzentoPOS2.Core.Exceptions;
using DzentoPOS2.Core.ValueObjects.Inventory;
using DzentoPOS2.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DzentoPOS2.App.Services.Inventory;

public sealed class StockService : IStockService
{
    public const string AllowNegativeStockKey = "inventory.allow_negative_stock";

    private readonly AppDbContext _context;
    private readonly IBusinessClock _clock;
    private readonly IAuthorizationGuard? _guard;
    private readonly ILogger<StockService>? _logger;

    public StockService(AppDbContext context, IBusinessClock clock)
        : this(context, clock, guard: null, logger: null)
    {
    }

    public StockService(AppDbContext context, IBusinessClock clock, IAuthorizationGuard? guard)
        : this(context, clock, guard, logger: null)
    {
    }

    public StockService(AppDbContext context, IBusinessClock clock, IAuthorizationGuard? guard, ILogger<StockService>? logger, IBatchService? batchService = null)
    {
        _context = context;
        _clock = clock;
        _guard = guard;
        _logger = logger;
        _batchService = batchService;
    }

    private readonly IBatchService? _batchService;

    public Task ApplySaleAsync(
        Guid invoiceId,
        string invoiceNumber,
        Guid warehouseId,
        IReadOnlyCollection<StockChange> lines,
        Guid? userId,
        CancellationToken cancellationToken = default)
    {
        return ApplyDeltaAsync(
            invoiceId,
            "SalesInvoice",
            invoiceNumber,
            warehouseId,
            lines,
            MovementType.SaleOut,
            -1,
            userId,
            cancellationToken);
    }

    public Task ApplyPurchaseAsync(
        Guid invoiceId,
        string invoiceNumber,
        Guid warehouseId,
        IReadOnlyCollection<StockChange> lines,
        Guid? userId,
        CancellationToken cancellationToken = default)
    {
        return ApplyDeltaAsync(
            invoiceId,
            "PurchaseInvoice",
            invoiceNumber,
            warehouseId,
            lines,
            MovementType.PurchaseIn,
            1,
            userId,
            cancellationToken);
    }

    public Task ReverseAsync(
        Guid sourceId,
        string sourceType,
        string referenceNumber,
        Guid warehouseId,
        IReadOnlyCollection<StockChange> lines,
        MovementType movementType,
        Guid? userId,
        CancellationToken cancellationToken = default)
    {
        var direction = IsIncoming(movementType) ? 1 : -1;

        return ApplyDeltaAsync(
            sourceId,
            sourceType,
            referenceNumber,
            warehouseId,
            lines,
            movementType,
            direction,
            userId,
            cancellationToken);
    }

    public async Task AdjustAsync(
        Guid productId,
        Guid warehouseId,
        decimal targetQuantity,
        string reason,
        Guid? userId,
        bool isOpening,
        CancellationToken cancellationToken = default)
    {
        if (_guard is not null)
            await _guard.DemandAsync(Permissions.InventoryAdjust);

        if (targetQuantity < 0)
            throw new DomainException("Stock quantity cannot be negative");

        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == productId,
                cancellationToken)
            ?? throw new DomainException("Product not found");

        if (!product.TrackStock)
            return;

        var variantId = await GetOrCreateDefaultVariantIdAsync(productId, cancellationToken);

        var stock = await GetTrackedStockAsync(
            productId,
            variantId,
            warehouseId,
            cancellationToken);

        var before = stock?.Quantity.Value ?? 0;

        if (stock is null)
        {
            stock = new ProductStock
            {
                ProductId = productId,
                VariantId = variantId,
                WarehouseId = warehouseId,
                Quantity = new Quantity(targetQuantity),
                Version = 0
            };

            await _context.ProductStocks.AddAsync(stock, cancellationToken);
        }
        else
        {
            stock.Quantity = new Quantity(targetQuantity);
            stock.Version++;
        }

        if (before == targetQuantity)
            return;

        var movementType = isOpening
            ? MovementType.Opening
            : targetQuantity > before
                ? MovementType.StockAdjustmentIncrease
                : MovementType.StockAdjustmentDecrease;

        await AddMovementAsync(
            productId,
            variantId,
            warehouseId,
            Math.Abs(targetQuantity - before),
            before,
            targetQuantity,
            movementType,
            "StockAdjustment",
            stock.Id,
            reason,
            userId,
            cancellationToken);
    }

    public async Task<decimal> GetAvailableAsync(
        Guid productId,
        Guid warehouseId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ProductStocks
            .AsNoTracking()
            .Where(x => x.ProductId == productId && x.WarehouseId == warehouseId)
            .Select(x => x.Quantity.Value)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task ApplyDeltaAsync(
        Guid sourceId,
        string sourceType,
        string referenceNumber,
        Guid warehouseId,
        IReadOnlyCollection<StockChange> lines,
        MovementType movementType,
        int direction,
        Guid? userId,
        CancellationToken cancellationToken)
    {
        var aggregated = lines
            .GroupBy(x => (x.ProductId, x.VariantId))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        if (aggregated.Count == 0)
            return;

        var productIds = aggregated.Keys.Select(k => k.ProductId).Distinct().ToList();

        var products = await _context.Products
            .AsNoTracking()
            .Where(x => productIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Name, x.TrackStock })
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        foreach (var (key, quantity) in aggregated)
        {
            var productId = key.ProductId;
            var variantId = key.VariantId == Guid.Empty
                ? await GetOrCreateDefaultVariantIdAsync(productId, cancellationToken)
                : key.VariantId;

            if (quantity <= 0)
                throw new DomainException("Stock movement quantity must be greater than zero");

            if (!products.TryGetValue(productId, out var product))
                throw new DomainException("Product not found");

            if (!product.TrackStock)
                continue;

            var stock = await GetTrackedStockAsync(
                productId,
                variantId,
                warehouseId,
                cancellationToken);

            var before = stock?.Quantity.Value ?? 0;
            var after = before + direction * quantity;

            if (after < 0)
            {
                // Insufficient stock does not block the sale by default; warn only.
                _logger?.LogWarning(
                    "Insufficient stock for '{Product}'. Available: {Available}, requested: {Requested}. Allowing negative stock.",
                    product.Name, before, quantity);
            }

            if (stock is null)
            {
                stock = new ProductStock
                {
                    ProductId = productId,
                    VariantId = variantId,
                    WarehouseId = warehouseId,
                    Quantity = after < 0
                        ? Quantity.FromSigned(after)
                        : new Quantity(after),
                    Version = 0
                };

                await _context.ProductStocks.AddAsync(stock, cancellationToken);
            }
            else
            {
                stock.Quantity = after < 0
                    ? Quantity.FromSigned(after)
                    : new Quantity(after);
                stock.Version++;
            }

            await AddMovementAsync(
                productId,
                variantId,
                warehouseId,
                quantity,
                before,
                after,
                movementType,
                sourceType,
                sourceId,
                referenceNumber,
                userId,
                cancellationToken);

            // Track batches for variant with batch tracking enabled
            var productVariant = await _context.ProductVariants
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == variantId, cancellationToken);
            if (productVariant?.TrackBatch == true && _batchService is not null)
            {
                if (direction > 0)
                {
                    // Purchase/return: increase batch quantity
                    var batchNumber = referenceNumber;
                    var existingBatch = await _context.Batches
                        .FirstOrDefaultAsync(b => b.VariantId == variantId && b.BatchNumber == batchNumber, cancellationToken);
                    if (existingBatch != null)
                    {
                        existingBatch.Quantity += quantity;
                    }
                    else
                    {
                        await _context.Batches.AddAsync(new Batch
                        {
                            VariantId = variantId,
                            WarehouseId = warehouseId,
                            BatchNumber = batchNumber,
                            Quantity = quantity,
                            UnitCost = 0,
                            ReceivedAt = _clock.UtcNow
                        }, cancellationToken);
                    }
                }
                else
                {
                    // Sale/adjustment: allocate from batches (FEFO)
                    var remainingQuantity = quantity;
                    var batches = await _context.Batches
                        .AsNoTracking()
                        .Where(b => b.VariantId == variantId && b.WarehouseId == warehouseId && b.Quantity > 0)
                        .OrderBy(b => b.ExpiryDate ?? DateTime.MaxValue)
                        .ThenBy(b => b.ReceivedAt)
                        .ToListAsync(cancellationToken);

                    foreach (var batch in batches)
                    {
                        if (remainingQuantity <= 0) break;
                        var take = Math.Min(remainingQuantity, batch.Quantity);
                        batch.Quantity -= take;
                        remainingQuantity -= take;
                    }

                    await _context.SaveChangesAsync(cancellationToken);
                }
            }
        }
    }

    private async Task<ProductStock?> GetTrackedStockAsync(
        Guid productId,
        Guid variantId,
        Guid warehouseId,
        CancellationToken cancellationToken)
    {
        return await _context.ProductStocks
            .FirstOrDefaultAsync(
                x => x.ProductId == productId && x.VariantId == variantId && x.WarehouseId == warehouseId,
                cancellationToken);
    }

    private async Task AddMovementAsync(
        Guid productId,
        Guid variantId,
        Guid warehouseId,
        decimal quantity,
        decimal before,
        decimal after,
        MovementType movementType,
        string sourceType,
        Guid sourceId,
        string referenceNumber,
        Guid? userId,
        CancellationToken cancellationToken)
    {
        await _context.InventoryTransactions.AddAsync(new InventoryTransaction
        {
            ProductId = productId,
            VariantId = variantId,
            WarehouseId = warehouseId,
            Quantity = new Quantity(quantity),
            TransactionType = movementType,
            QuantityBefore = before,
            QuantityAfter = after,
            SourceType = sourceType,
            SourceId = sourceId,
            UserId = userId,
            ReferenceNumber = referenceNumber,
            CreatedAt = _clock.UtcNow
        }, cancellationToken);
    }

    private async Task<bool> IsNegativeStockAllowedAsync(
        CancellationToken cancellationToken)
    {
        var value = await _context.AppSettings
            .AsNoTracking()
            .Where(x => x.Key == AllowNegativeStockKey)
            .Select(x => x.Value)
            .FirstOrDefaultAsync(cancellationToken);

        return bool.TryParse(value, out var allowed) && allowed;
    }

    private async Task<Guid> GetOrCreateDefaultVariantIdAsync(Guid productId, CancellationToken cancellationToken)
    {
        var existing = await _context.ProductVariants
            .AsNoTracking()
            .Where(v => v.ProductId == productId && v.IsDefault)
            .Select(v => v.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (existing != Guid.Empty)
            return existing;

        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product is null)
            throw new DomainException("Product not found");

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
        await _context.ProductVariants.AddAsync(variant, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return variant.Id;
    }

    private static bool IsIncoming(MovementType movementType)
    {
        return movementType is
            MovementType.Purchase or
            MovementType.PurchaseIn or
            MovementType.SaleReturn or
            MovementType.SaleVoidIn or
            MovementType.StockAdjustmentIncrease or
            MovementType.StockTransferIn or
            MovementType.Opening;
    }
}

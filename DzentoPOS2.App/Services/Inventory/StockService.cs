using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Core.Enums;
using DzentoPOS2.Core.Exceptions;
using DzentoPOS2.Core.ValueObjects.Inventory;
using DzentoPOS2.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.App.Services.Inventory;

public sealed class StockService : IStockService
{
    public const string AllowNegativeStockKey = "inventory.allow_negative_stock";

    private readonly AppDbContext _context;
    private readonly IBusinessClock _clock;

    public StockService(AppDbContext context, IBusinessClock clock)
    {
        _context = context;
        _clock = clock;
    }

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

        var stock = await GetTrackedStockAsync(
            productId,
            warehouseId,
            cancellationToken);

        var before = stock?.Quantity.Value ?? 0;

        if (stock is null)
        {
            stock = new ProductStock
            {
                ProductId = productId,
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
            .GroupBy(x => x.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        if (aggregated.Count == 0)
            return;

        var productIds = aggregated.Keys.ToList();

        var products = await _context.Products
            .AsNoTracking()
            .Where(x => productIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Name, x.TrackStock })
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var allowNegative = await IsNegativeStockAllowedAsync(cancellationToken);

        foreach (var (productId, quantity) in aggregated)
        {
            if (quantity <= 0)
                throw new DomainException("Stock movement quantity must be greater than zero");

            if (!products.TryGetValue(productId, out var product))
                throw new DomainException("Product not found");

            if (!product.TrackStock)
                continue;

            var stock = await GetTrackedStockAsync(
                productId,
                warehouseId,
                cancellationToken);

            var before = stock?.Quantity.Value ?? 0;
            var after = before + direction * quantity;

            if (after < 0 && !allowNegative)
            {
                throw new DomainException(
                    $"Insufficient stock for '{product.Name}'. Available: {before:N2}, requested: {quantity:N2}.");
            }

            if (stock is null)
            {
                stock = new ProductStock
                {
                    ProductId = productId,
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
        }
    }

    private async Task<ProductStock?> GetTrackedStockAsync(
        Guid productId,
        Guid warehouseId,
        CancellationToken cancellationToken)
    {
        return await _context.ProductStocks
            .FirstOrDefaultAsync(
                x => x.ProductId == productId && x.WarehouseId == warehouseId,
                cancellationToken);
    }

    private async Task AddMovementAsync(
        Guid productId,
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

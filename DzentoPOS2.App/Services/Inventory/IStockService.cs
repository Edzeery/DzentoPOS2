using DzentoPOS2.Core.Enums;

namespace DzentoPOS2.App.Services.Inventory;

public readonly record struct StockChange(Guid ProductId, decimal Quantity);

public interface IStockService
{
    Task ApplySaleAsync(
        Guid invoiceId,
        string invoiceNumber,
        Guid warehouseId,
        IReadOnlyCollection<StockChange> lines,
        Guid? userId,
        CancellationToken cancellationToken = default);

    Task ApplyPurchaseAsync(
        Guid invoiceId,
        string invoiceNumber,
        Guid warehouseId,
        IReadOnlyCollection<StockChange> lines,
        Guid? userId,
        CancellationToken cancellationToken = default);

    Task ReverseAsync(
        Guid sourceId,
        string sourceType,
        string referenceNumber,
        Guid warehouseId,
        IReadOnlyCollection<StockChange> lines,
        MovementType movementType,
        Guid? userId,
        CancellationToken cancellationToken = default);

    Task AdjustAsync(
        Guid productId,
        Guid warehouseId,
        decimal targetQuantity,
        string reason,
        Guid? userId,
        bool isOpening,
        CancellationToken cancellationToken = default);

    Task<decimal> GetAvailableAsync(
        Guid productId,
        Guid warehouseId,
        CancellationToken cancellationToken = default);
}

using DzentoPOS2.Shared.Contracts.DTOs.Businesses;

namespace DzentoPOS2.App.Services.Businesses;

/// <summary>
/// Manages the warehouses and cashboxes that belong to a branch. Neither is
/// ever deleted: they are only activated or deactivated, and the service
/// refuses to leave a branch without at least one active warehouse and one
/// active cashbox, or to deactivate a cashbox with an open session.
/// </summary>
public interface IBranchResourceService
{
    Task<List<WarehouseListItemDto>> GetWarehousesAsync(Guid branchId);

    Task<List<CashboxListItemDto>> GetCashboxesAsync(Guid branchId);

    Task<WarehouseListItemDto> CreateWarehouseAsync(CreateWarehouseDto dto);

    Task<CashboxListItemDto> CreateCashboxAsync(CreateCashboxDto dto);

    /// <summary>Activates or deactivates a warehouse (never deletes it).</summary>
    Task<WarehouseListItemDto> SetWarehouseActiveAsync(Guid id, bool isActive);

    /// <summary>Activates or deactivates a cashbox (never deletes it).</summary>
    Task<CashboxListItemDto> SetCashboxActiveAsync(Guid id, bool isActive);
}

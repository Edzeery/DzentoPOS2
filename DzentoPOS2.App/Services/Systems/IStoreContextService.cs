namespace DzentoPOS2.App.Services.Systems;

public interface IStoreContextService
{
    Task<Guid> GetDefaultBranchIdAsync(CancellationToken cancellationToken = default);

    Task<Guid> GetDefaultWarehouseIdAsync(
        Guid branchId,
        CancellationToken cancellationToken = default);

    Task<Guid> GetDefaultCashboxIdAsync(
        Guid branchId,
        CancellationToken cancellationToken = default);
}

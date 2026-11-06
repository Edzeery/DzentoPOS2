namespace DzentoPOS2.App.Services.Systems;

/// <summary>
/// Minimal, immutable description of the store's default branch.
/// </summary>
public sealed record StoreBranchInfo(Guid Id, string Name, bool IsMainBranch);

public interface IStoreContextService
{
    Task<Guid> GetDefaultBranchIdAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Real branch (id + name) used by the shell header. Returns <c>null</c>
    /// when no active branch exists.
    /// </summary>
    Task<StoreBranchInfo?> GetDefaultBranchAsync(CancellationToken cancellationToken = default);

    Task<Guid> GetDefaultWarehouseIdAsync(
        Guid branchId,
        CancellationToken cancellationToken = default);

    Task<Guid> GetDefaultCashboxIdAsync(
        Guid branchId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Drops the cached ids. Call when branch/warehouse/cashbox configuration
    /// changes.
    /// </summary>
    void Invalidate();
}

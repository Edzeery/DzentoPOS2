using DzentoPOS2.App.Session;
using DzentoPOS2.Core.Exceptions;
using DzentoPOS2.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.App.Services.Systems;

/// <summary>
/// Read-only accessor for the store's default branch / warehouse / cashbox.
///
/// It never inserts rows: the store is created once at startup by
/// <c>StoreBootstrapper</c>. When something is missing it throws
/// <see cref="StoreNotInitializedException"/> instead of writing mid-sale.
/// Results are cached (per resolved branch) and can be dropped with
/// <see cref="Invalidate"/> when the branch configuration changes.
/// </summary>
public sealed class StoreContextService : IStoreContextService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    private readonly object _gate = new();

    private StoreBranchInfo? _branchCache;
    private Guid? _branchCacheUserBranchId;
    private bool _branchCacheValid;

    private readonly Dictionary<Guid, Guid> _warehouseCache = new();
    private readonly Dictionary<Guid, Guid> _cashboxCache = new();

    public StoreContextService(
        AppDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public void Invalidate()
    {
        lock (_gate)
        {
            _branchCache = null;
            _branchCacheUserBranchId = null;
            _branchCacheValid = false;
            _warehouseCache.Clear();
            _cashboxCache.Clear();
        }
    }

    public async Task<Guid> GetDefaultBranchIdAsync(
        CancellationToken cancellationToken = default)
    {
        var branch = await GetDefaultBranchAsync(cancellationToken)
            ?? throw new StoreNotInitializedException();

        return branch.Id;
    }

    public async Task<StoreBranchInfo?> GetDefaultBranchAsync(
        CancellationToken cancellationToken = default)
    {
        var preferred = _currentUser.BranchId;

        lock (_gate)
        {
            if (_branchCacheValid && _branchCacheUserBranchId == preferred)
                return _branchCache;
        }

        var branch = await ResolveBranchAsync(preferred, cancellationToken);

        lock (_gate)
        {
            _branchCache = branch;
            _branchCacheUserBranchId = preferred;
            _branchCacheValid = true;
        }

        return branch;
    }

    public async Task<Guid> GetDefaultWarehouseIdAsync(
        Guid branchId,
        CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            if (_warehouseCache.TryGetValue(branchId, out var cached))
                return cached;
        }

        var id = await _context.Warehouses
            .AsNoTracking()
            .Where(x => x.BranchId == branchId && x.IsActive)
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new StoreNotInitializedException(
                "No active warehouse is configured for the store branch.");

        lock (_gate)
            _warehouseCache[branchId] = id;

        return id;
    }

    public async Task<Guid> GetDefaultCashboxIdAsync(
        Guid branchId,
        CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            if (_cashboxCache.TryGetValue(branchId, out var cached))
                return cached;
        }

        var id = await _context.Cashboxes
            .AsNoTracking()
            .Where(x => x.BranchId == branchId && x.IsActive)
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new StoreNotInitializedException(
                "No active cashbox is configured for the store branch.");

        lock (_gate)
            _cashboxCache[branchId] = id;

        return id;
    }

    private async Task<StoreBranchInfo?> ResolveBranchAsync(
        Guid? preferred,
        CancellationToken cancellationToken)
    {
        if (preferred is not null)
        {
            var preferredBranch = await _context.Branches
                .AsNoTracking()
                .Where(x => x.Id == preferred && x.IsActive)
                .Select(x => new StoreBranchInfo(x.Id, x.Name, x.IsMainBranch))
                .FirstOrDefaultAsync(cancellationToken);

            if (preferredBranch is not null)
                return preferredBranch;
        }

        return await _context.Branches
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.IsMainBranch)
            .ThenBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Select(x => new StoreBranchInfo(x.Id, x.Name, x.IsMainBranch))
            .FirstOrDefaultAsync(cancellationToken);
    }
}

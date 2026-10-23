using DzentoPOS2.App.Session;
using DzentoPOS2.Core.Entities.Accounting;
using DzentoPOS2.Core.Entities.Businesses;
using DzentoPOS2.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.App.Services.Systems;

public sealed class StoreContextService : IStoreContextService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IBusinessClock _clock;

    public StoreContextService(
        AppDbContext context,
        ICurrentUserService currentUser,
        IBusinessClock clock)
    {
        _context = context;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Guid> GetDefaultBranchIdAsync(
        CancellationToken cancellationToken = default)
    {
        var preferredId = _currentUser.BranchId;

        if (preferredId is not null)
        {
            var preferredExists = await _context.Branches
                .AsNoTracking()
                .AnyAsync(
                    x => x.Id == preferredId && x.IsActive,
                    cancellationToken);

            if (preferredExists)
                return preferredId.Value;
        }

        var branch = await _context.Branches
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.IsMainBranch)
            .ThenBy(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (branch is not null)
            return branch.Id;

        var created = new Branch
        {
            Name = "Main Branch",
            IsMainBranch = true,
            IsActive = true,
            CreatedAt = _clock.UtcNow
        };

        await _context.Branches.AddAsync(created, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return created.Id;
    }

    public async Task<Guid> GetDefaultWarehouseIdAsync(
        Guid branchId,
        CancellationToken cancellationToken = default)
    {
        var warehouse = await _context.Warehouses
            .AsNoTracking()
            .Where(x => x.BranchId == branchId && x.IsActive)
            .OrderBy(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (warehouse is not null)
            return warehouse.Id;

        var created = new Warehouse
        {
            Name = "Main Warehouse",
            BranchId = branchId,
            IsActive = true,
            CreatedAt = _clock.UtcNow
        };

        await _context.Warehouses.AddAsync(created, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return created.Id;
    }

    public async Task<Guid> GetDefaultCashboxIdAsync(
        Guid branchId,
        CancellationToken cancellationToken = default)
    {
        var cashbox = await _context.Cashboxes
            .AsNoTracking()
            .Where(x => x.BranchId == branchId && x.IsActive)
            .OrderBy(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (cashbox is not null)
            return cashbox.Id;

        var created = new Cashbox
        {
            Name = "Main Cashbox",
            BranchId = branchId,
            IsActive = true,
            CreatedAt = _clock.UtcNow
        };

        await _context.Cashboxes.AddAsync(created, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return created.Id;
    }
}

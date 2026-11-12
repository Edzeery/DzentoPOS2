using DzentoPOS2.App.Authorization;
using DzentoPOS2.Core.Constants;
using DzentoPOS2.Core.Entities.Accounting;
using DzentoPOS2.Core.Entities.Businesses;
using DzentoPOS2.Core.Enums;
using DzentoPOS2.Core.Exceptions;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Businesses;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.App.Services.Businesses;

/// <summary>
/// Branch warehouse and cashbox management. Every operation is guarded by a
/// branch permission; <see cref="Permissions.BranchesManage"/> acts as the
/// umbrella grant while the granular codes let a single action be delegated.
///
/// <para>
/// Nothing is ever deleted. Deactivation is refused for the last active
/// warehouse/cashbox of a branch, and for a cashbox with an open cash session,
/// so a branch can never end up unable to store stock or hold money.
/// </para>
/// </summary>
public class BranchResourceService : IBranchResourceService
{
    private readonly AppDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationGuard _guard;
    private readonly IAuditService _audit;

    public BranchResourceService(
        AppDbContext context,
        IUnitOfWork unitOfWork,
        IAuthorizationGuard guard,
        IAuditService audit)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _guard = guard;
        _audit = audit;
    }

    public async Task<List<WarehouseListItemDto>> GetWarehousesAsync(Guid branchId)
    {
        await DemandAsync(Permissions.BranchesView);

        return await _context.Warehouses
            .AsNoTracking()
            .Where(w => w.BranchId == branchId && !w.IsDeleted)
            .OrderByDescending(w => w.IsActive)
            .ThenBy(w => w.Name)
            .Select(w => new WarehouseListItemDto
            {
                Id = w.Id,
                BranchId = w.BranchId,
                Name = w.Name,
                Location = w.Location,
                IsActive = w.IsActive
            })
            .ToListAsync();
    }

    public async Task<List<CashboxListItemDto>> GetCashboxesAsync(Guid branchId)
    {
        await DemandAsync(Permissions.BranchesView);

        var cashboxes = await _context.Cashboxes
            .AsNoTracking()
            .Where(c => c.BranchId == branchId && !c.IsDeleted)
            .OrderByDescending(c => c.IsActive)
            .ThenBy(c => c.Name)
            .ToListAsync();

        var ids = cashboxes.Select(c => c.Id).ToList();
        var withOpenSession = await _context.CashSessions
            .AsNoTracking()
            .Where(s => s.Status == CashSessionStatus.Open && ids.Contains(s.CashboxId))
            .Select(s => s.CashboxId)
            .Distinct()
            .ToListAsync();
        var openSet = withOpenSession.ToHashSet();

        return cashboxes
            .Select(c => new CashboxListItemDto
            {
                Id = c.Id,
                BranchId = c.BranchId,
                Name = c.Name,
                CurrentBalance = c.CurrentBalance,
                IsActive = c.IsActive,
                HasOpenSession = openSet.Contains(c.Id)
            })
            .ToList();
    }

    public async Task<WarehouseListItemDto> CreateWarehouseAsync(CreateWarehouseDto dto)
    {
        await DemandAsync(Permissions.BranchesEdit);

        var name = (dto.Name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new CodedDomainException(
                DomainErrorCodes.WarehouseNameRequired,
                "A warehouse name is required.");
        }

        await EnsureBranchExistsAsync(dto.BranchId);

        if (await _context.Warehouses.AnyAsync(w =>
                w.BranchId == dto.BranchId && !w.IsDeleted && w.Name == name))
        {
            throw new CodedDomainException(
                DomainErrorCodes.WarehouseDuplicateName,
                $"A warehouse named '{name}' already exists in this branch.");
        }

        var warehouse = new Warehouse
        {
            Name = name,
            Location = string.IsNullOrWhiteSpace(dto.Location) ? null : dto.Location!.Trim(),
            BranchId = dto.BranchId,
            IsActive = dto.IsActive
        };

        _context.Warehouses.Add(warehouse);
        await _unitOfWork.SaveChangesAsync();

        await _audit.LogAsync("WarehouseCreated", "Warehouse", newValues: warehouse.Name);

        return MapWarehouse(warehouse);
    }

    public async Task<CashboxListItemDto> CreateCashboxAsync(CreateCashboxDto dto)
    {
        await DemandAsync(Permissions.BranchesEdit);

        var name = (dto.Name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new CodedDomainException(
                DomainErrorCodes.CashboxNameRequired,
                "A cashbox name is required.");
        }

        await EnsureBranchExistsAsync(dto.BranchId);

        if (await _context.Cashboxes.AnyAsync(c =>
                c.BranchId == dto.BranchId && !c.IsDeleted && c.Name == name))
        {
            throw new CodedDomainException(
                DomainErrorCodes.CashboxDuplicateName,
                $"A cashbox named '{name}' already exists in this branch.");
        }

        var cashbox = new Cashbox
        {
            Name = name,
            BranchId = dto.BranchId,
            IsActive = dto.IsActive
        };

        _context.Cashboxes.Add(cashbox);
        await _unitOfWork.SaveChangesAsync();

        await _audit.LogAsync("CashboxCreated", "Cashbox", newValues: cashbox.Name);

        return MapCashbox(cashbox, hasOpenSession: false);
    }

    public async Task<WarehouseListItemDto> SetWarehouseActiveAsync(Guid id, bool isActive)
    {
        await DemandAsync(isActive ? Permissions.BranchesEdit : Permissions.BranchesDeactivate);

        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted)
            ?? throw new CodedDomainException(
                DomainErrorCodes.WarehouseNotFound,
                $"Warehouse {id} was not found.");

        if (warehouse.IsActive == isActive)
            return MapWarehouse(warehouse);

        if (!isActive)
        {
            var activeCount = await _context.Warehouses
                .CountAsync(w => w.BranchId == warehouse.BranchId && w.IsActive && !w.IsDeleted);
            if (activeCount <= 1)
            {
                throw new CodedDomainException(
                    DomainErrorCodes.WarehouseLastActive,
                    "The last active warehouse of a branch cannot be deactivated.");
            }
        }

        warehouse.IsActive = isActive;
        await _unitOfWork.SaveChangesAsync();

        await _audit.LogAsync(
            isActive ? "WarehouseActivated" : "WarehouseDeactivated",
            "Warehouse",
            newValues: warehouse.Name);

        return MapWarehouse(warehouse);
    }

    public async Task<CashboxListItemDto> SetCashboxActiveAsync(Guid id, bool isActive)
    {
        await DemandAsync(isActive ? Permissions.BranchesEdit : Permissions.BranchesDeactivate);

        var cashbox = await _context.Cashboxes
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted)
            ?? throw new CodedDomainException(
                DomainErrorCodes.CashboxNotFound,
                $"Cashbox {id} was not found.");

        if (cashbox.IsActive == isActive)
            return MapCashbox(cashbox, await HasOpenSessionAsync(cashbox.Id));

        if (!isActive)
        {
            var activeCount = await _context.Cashboxes
                .CountAsync(c => c.BranchId == cashbox.BranchId && c.IsActive && !c.IsDeleted);
            if (activeCount <= 1)
            {
                throw new CodedDomainException(
                    DomainErrorCodes.CashboxLastActive,
                    "The last active cashbox of a branch cannot be deactivated.");
            }

            if (await HasOpenSessionAsync(cashbox.Id))
            {
                throw new CodedDomainException(
                    DomainErrorCodes.CashboxHasOpenSession,
                    "A cashbox with an open cash session cannot be deactivated.");
            }
        }

        cashbox.IsActive = isActive;
        await _unitOfWork.SaveChangesAsync();

        await _audit.LogAsync(
            isActive ? "CashboxActivated" : "CashboxDeactivated",
            "Cashbox",
            newValues: cashbox.Name);

        return MapCashbox(cashbox, await HasOpenSessionAsync(cashbox.Id));
    }

    private async Task EnsureBranchExistsAsync(Guid branchId)
    {
        var exists = await _context.Branches
            .AnyAsync(b => b.Id == branchId && !b.IsDeleted);
        if (!exists)
        {
            throw new CodedDomainException(
                DomainErrorCodes.BranchNotFound,
                $"Branch {branchId} was not found.");
        }
    }

    private Task<bool> HasOpenSessionAsync(Guid cashboxId) =>
        _context.CashSessions
            .AnyAsync(s => s.CashboxId == cashboxId && s.Status == CashSessionStatus.Open);

    /// <summary>
    /// A service operation is allowed when the caller holds the granular
    /// permission or the umbrella <see cref="Permissions.BranchesManage"/>.
    /// </summary>
    private async Task DemandAsync(string granularPermission)
    {
        if (await _guard.IsAllowedAsync(Permissions.BranchesManage))
            return;

        await _guard.DemandAsync(granularPermission);
    }

    private static WarehouseListItemDto MapWarehouse(Warehouse w) => new()
    {
        Id = w.Id,
        BranchId = w.BranchId,
        Name = w.Name,
        Location = w.Location,
        IsActive = w.IsActive
    };

    private static CashboxListItemDto MapCashbox(Cashbox c, bool hasOpenSession) => new()
    {
        Id = c.Id,
        BranchId = c.BranchId,
        Name = c.Name,
        CurrentBalance = c.CurrentBalance,
        IsActive = c.IsActive,
        HasOpenSession = hasOpenSession
    };
}

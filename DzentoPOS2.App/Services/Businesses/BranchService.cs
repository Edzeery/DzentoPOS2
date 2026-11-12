using DzentoPOS2.App.Authorization;
using DzentoPOS2.Core.Constants;
using DzentoPOS2.Core.Entities.Accounting;
using DzentoPOS2.Core.Entities.Businesses;
using DzentoPOS2.Core.Exceptions;
using DzentoPOS2.Core.ValueObjects.Business;
using DzentoPOS2.Core.ValueObjects.Identity;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Businesses.Interfaces;
using DzentoPOS2.Data.Repositories.Common.Interfaces;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Shared.Contracts.DTOs.Businesses;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.App.Services.Businesses;

/// <summary>
/// Branch management service. Every operation is guarded by a branch
/// permission; <see cref="Permissions.BranchesManage"/> acts as the umbrella
/// grant for a branch manager while the granular codes let a supervisor be
/// given a single action.
///
/// <para>
/// Creation is atomic: the branch, its default warehouse and its default
/// cashbox are inserted inside one explicit transaction. Any failure rolls the
/// whole thing back, so a half-created branch can never be committed.
/// </para>
/// </summary>
public class BranchService : IBranchService
{
    private readonly IBranchRepository _branchRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppDbContext _context;
    private readonly IAuthorizationGuard _guard;
    private readonly IAuditService _audit;

    public BranchService(
        IBranchRepository branchRepo,
        IUnitOfWork unitOfWork,
        AppDbContext context,
        IAuthorizationGuard guard,
        IAuditService audit)
    {
        _branchRepo = branchRepo;
        _unitOfWork = unitOfWork;
        _context = context;
        _guard = guard;
        _audit = audit;
    }

    public async Task<List<BranchListItemDto>> GetAllAsync()
    {
        await DemandAsync(Permissions.BranchesView);

        var branches = await _branchRepo.GetAllOrderedAsync();

        var warehouseCounts = await _context.Warehouses
            .AsNoTracking()
            .Where(w => !w.IsDeleted)
            .GroupBy(w => w.BranchId)
            .Select(g => new { BranchId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.BranchId, x => x.Count);

        var cashboxCounts = await _context.Cashboxes
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .GroupBy(c => c.BranchId)
            .Select(g => new { BranchId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.BranchId, x => x.Count);

        var branchesWithOpenSession = await _context.CashSessions
            .AsNoTracking()
            .Where(s => s.Status == Core.Enums.CashSessionStatus.Open)
            .Join(
                _context.Cashboxes.AsNoTracking().Where(c => !c.IsDeleted),
                s => s.CashboxId,
                c => c.Id,
                (_, c) => c.BranchId)
            .Distinct()
            .ToListAsync();
        var openSessionSet = branchesWithOpenSession.ToHashSet();

        return branches
            .Select(b =>
            {
                var dto = Map(b);
                return new BranchListItemDto
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    Country = dto.Country,
                    City = dto.City,
                    Street = dto.Street,
                    PhoneNumber = dto.PhoneNumber,
                    IsMainBranch = dto.IsMainBranch,
                    IsActive = dto.IsActive,
                    CreatedAt = dto.CreatedAt,
                    WarehousesCount = warehouseCounts.GetValueOrDefault(b.Id),
                    CashboxesCount = cashboxCounts.GetValueOrDefault(b.Id),
                    HasOpenCashSession = openSessionSet.Contains(b.Id)
                };
            })
            .ToList();
    }

    public async Task<BranchDto?> GetByIdAsync(Guid id)
    {
        await DemandAsync(Permissions.BranchesView);

        var branch = await _context.Branches
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);

        return branch is null ? null : Map(branch);
    }

    public async Task<BranchDto> CreateAsync(CreateBranchDto dto)
    {
        await DemandAsync(Permissions.BranchesCreate);

        var name = (dto.Name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new CodedDomainException(
                DomainErrorCodes.BranchNameRequired,
                "A branch name is required.");
        }

        if (await NameExistsAsync(name, excludeId: null))
        {
            throw new CodedDomainException(
                DomainErrorCodes.BranchDuplicateName,
                $"A branch named '{name}' already exists.");
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // Only the very first branch in the store becomes the main branch.
            var isFirstBranch = !await _context.Branches.AnyAsync(b => !b.IsDeleted);

            var branch = new Branch
            {
                Name = name,
                Address = BuildAddress(dto.Country, dto.City, dto.Street),
                PhoneNumber = BuildPhoneNumber(dto.PhoneNumber),
                IsMainBranch = isFirstBranch,
                IsActive = dto.IsActive
            };

            var warehouse = new Warehouse
            {
                Name = $"{name} — {AppStrings.MainWarehouse}",
                BranchId = branch.Id,
                IsActive = true
            };

            var cashbox = new Cashbox
            {
                Name = $"{name} — {AppStrings.MainCashbox}",
                BranchId = branch.Id,
                IsActive = true
            };

            _context.Branches.Add(branch);
            _context.Warehouses.Add(warehouse);
            _context.Cashboxes.Add(cashbox);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            await _audit.LogAsync(
                "BranchCreated",
                "Branch",
                newValues: branch.Name);

            return Map(branch);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<BranchDto> UpdateAsync(UpdateBranchDto dto)
    {
        await DemandAsync(Permissions.BranchesEdit);

        var branch = await GetTrackedAsync(dto.Id);

        var name = (dto.Name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new CodedDomainException(
                DomainErrorCodes.BranchNameRequired,
                "A branch name is required.");
        }

        if (await NameExistsAsync(name, excludeId: dto.Id))
        {
            throw new CodedDomainException(
                DomainErrorCodes.BranchDuplicateName,
                $"A branch named '{name}' already exists.");
        }

        branch.Name = name;
        branch.Address = BuildAddress(dto.Country, dto.City, dto.Street);
        branch.PhoneNumber = BuildPhoneNumber(dto.PhoneNumber);

        await _unitOfWork.SaveChangesAsync();

        await _audit.LogAsync(
            "BranchUpdated",
            "Branch",
            newValues: branch.Name);

        return Map(branch);
    }

    public async Task<BranchDto> SetActiveAsync(Guid id, bool isActive)
    {
        // Reactivating is an edit; deactivating is its own permission.
        await DemandAsync(isActive
            ? Permissions.BranchesEdit
            : Permissions.BranchesDeactivate);

        var branch = await GetTrackedAsync(id);

        if (branch.IsActive == isActive)
            return Map(branch);

        if (!isActive)
        {
            if (branch.IsMainBranch)
            {
                throw new CodedDomainException(
                    DomainErrorCodes.BranchMainCannotDeactivate,
                    "The main branch cannot be deactivated.");
            }

            var activeCount = await _context.Branches
                .CountAsync(b => b.IsActive && !b.IsDeleted);
            if (activeCount <= 1)
            {
                throw new CodedDomainException(
                    DomainErrorCodes.BranchLastActive,
                    "The last active branch cannot be deactivated.");
            }

            if (await _branchRepo.HasOpenCashSessionAsync(branch.Id))
            {
                throw new CodedDomainException(
                    DomainErrorCodes.BranchHasOpenSession,
                    "A branch with an open cash session cannot be deactivated.");
            }
        }

        branch.IsActive = isActive;
        await _unitOfWork.SaveChangesAsync();

        await _audit.LogAsync(
            isActive ? "BranchActivated" : "BranchDeactivated",
            "Branch",
            newValues: branch.Name);

        return Map(branch);
    }

    private async Task<Branch> GetTrackedAsync(Guid id)
    {
        var branch = await _context.Branches
            .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);

        if (branch is null)
        {
            throw new CodedDomainException(
                DomainErrorCodes.BranchNotFound,
                $"Branch {id} was not found.");
        }

        return branch;
    }

    private Task<bool> NameExistsAsync(string name, Guid? excludeId)
    {
        var query = _context.Branches.Where(b => !b.IsDeleted && b.Name == name);
        if (excludeId.HasValue)
            query = query.Where(b => b.Id != excludeId.Value);

        return query.AnyAsync();
    }

    /// <summary>
    /// A service operation is allowed when the caller holds the granular
    /// permission or the umbrella <see cref="Permissions.BranchesManage"/>.
    /// The guard still audits a denial.
    /// </summary>
    private async Task DemandAsync(string granularPermission)
    {
        if (await _guard.IsAllowedAsync(Permissions.BranchesManage))
            return;

        await _guard.DemandAsync(granularPermission);
    }

    private static Address? BuildAddress(string? country, string? city, string? street)
    {
        var c = country?.Trim();
        var ci = city?.Trim();
        var st = street?.Trim();

        if (string.IsNullOrWhiteSpace(c) &&
            string.IsNullOrWhiteSpace(ci) &&
            string.IsNullOrWhiteSpace(st))
        {
            return null;
        }

        return new Address(c ?? string.Empty, ci ?? string.Empty, st ?? string.Empty);
    }

    private static PhoneNumber? BuildPhoneNumber(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : new PhoneNumber(value.Trim());

    private static BranchDto Map(Branch branch) => new()
    {
        Id = branch.Id,
        Name = branch.Name,
        Country = branch.Address?.Country,
        City = branch.Address?.City,
        Street = branch.Address?.Street,
        PhoneNumber = branch.PhoneNumber?.Value,
        IsMainBranch = branch.IsMainBranch,
        IsActive = branch.IsActive,
        CreatedAt = branch.CreatedAt
    };
}

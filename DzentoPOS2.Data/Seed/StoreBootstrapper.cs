using DzentoPOS2.Core.Entities.Accounting;
using DzentoPOS2.Core.Entities.Businesses;
using DzentoPOS2.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Seed;

/// <summary>
/// Describes what the bootstrap changed, so callers can log/report it.
/// </summary>
public sealed class StoreBootstrapResult
{
    public bool BranchCreated { get; set; }

    public bool MainBranchPromoted { get; set; }

    public bool BranchActivated { get; set; }

    public bool WarehouseCreated { get; set; }

    public bool WarehouseReparented { get; set; }

    public bool CashboxCreated { get; set; }

    public bool CashboxReparented { get; set; }

    public bool CashboxKeptForDependents { get; set; }

    public Guid BranchId { get; set; }

    public Guid WarehouseId { get; set; }

    public Guid CashboxId { get; set; }

    public bool Changed =>
        BranchCreated || MainBranchPromoted || BranchActivated ||
        WarehouseCreated || WarehouseReparented ||
        CashboxCreated || CashboxReparented;

    public override string ToString()
    {
        var parts = new List<string>();
        if (BranchCreated) parts.Add("branch created");
        if (MainBranchPromoted) parts.Add("main branch promoted");
        if (BranchActivated) parts.Add("branch activated");
        if (WarehouseCreated) parts.Add("warehouse created");
        if (WarehouseReparented) parts.Add("warehouse reparented");
        if (CashboxCreated) parts.Add("cashbox created");
        if (CashboxReparented) parts.Add("cashbox reparented");
        if (CashboxKeptForDependents) parts.Add("cashbox kept because it has dependents");
        return parts.Count == 0 ? "store already initialized" : string.Join(", ", parts);
    }
}

/// <summary>
/// Idempotent store bootstrap. Runs once at startup (after migrations) and makes
/// sure the store has exactly one deterministic main branch, one active
/// warehouse and one active cashbox under it.
///
/// This replaces the lazy creation that used to live inside
/// <c>StoreContextService</c> (a getter with side effects executed in the middle
/// of a sale) and the disagreeing <see cref="WarehouseSeeder"/> /
/// <see cref="CashboxSeeder"/> branch creation.
/// </summary>
public static class StoreBootstrapper
{
    public const string DefaultBranchName = "الفرع الرئيسي";

    public const string DefaultWarehouseName = "المخزن الرئيسي";

    public const string DefaultCashboxName = "الصندوق الرئيسي";

    public static async Task<StoreBootstrapResult> EnsureInitializedAsync(
        AppDbContext context,
        CancellationToken cancellationToken = default)
    {
        var result = new StoreBootstrapResult();

        var branch = await EnsureMainBranchAsync(context, result, cancellationToken);
        result.BranchId = branch.Id;

        result.WarehouseId = await EnsureWarehouseAsync(context, branch, result, cancellationToken);
        result.CashboxId = await EnsureCashboxAsync(context, branch, result, cancellationToken);

        return result;
    }

    private static async Task<Branch> EnsureMainBranchAsync(
        AppDbContext context,
        StoreBootstrapResult result,
        CancellationToken cancellationToken)
    {
        // Deterministic choice: an existing main branch wins, otherwise the
        // oldest active branch, otherwise the oldest branch. Never create a
        // second branch when one already exists.
        var branch = await context.Branches
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.IsMainBranch)
            .ThenByDescending(x => x.IsActive)
            .ThenBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (branch is null)
        {
            branch = new Branch
            {
                Name = DefaultBranchName,
                IsMainBranch = true,
                IsActive = true
            };

            context.Branches.Add(branch);
            await context.SaveChangesAsync(cancellationToken);

            result.BranchCreated = true;
            return branch;
        }

        var dirty = false;

        if (!branch.IsMainBranch)
        {
            branch.IsMainBranch = true;
            result.MainBranchPromoted = true;
            dirty = true;
        }

        if (!branch.IsActive)
        {
            // The main branch must be usable by the sale path, which only
            // considers active branches.
            branch.IsActive = true;
            result.BranchActivated = true;
            dirty = true;
        }

        if (dirty)
            await context.SaveChangesAsync(cancellationToken);

        return branch;
    }

    private static async Task<Guid> EnsureWarehouseAsync(
        AppDbContext context,
        Branch branch,
        StoreBootstrapResult result,
        CancellationToken cancellationToken)
    {
        var existing = await context.Warehouses
            .Where(x => !x.IsDeleted && x.IsActive && x.BranchId == branch.Id)
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is not null)
            return existing.Id;

        var candidate = await context.Warehouses
            .Where(x => !x.IsDeleted && x.BranchId != branch.Id)
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (candidate is not null && !await WarehouseHasDependentsAsync(context, candidate.Id, cancellationToken))
        {
            candidate.BranchId = branch.Id;
            if (!candidate.IsActive)
                candidate.IsActive = true;

            await context.SaveChangesAsync(cancellationToken);
            result.WarehouseReparented = true;
            return candidate.Id;
        }

        var created = new Warehouse
        {
            Name = DefaultWarehouseName,
            BranchId = branch.Id,
            IsActive = true
        };

        context.Warehouses.Add(created);
        await context.SaveChangesAsync(cancellationToken);
        result.WarehouseCreated = true;

        return created.Id;
    }

    private static async Task<Guid> EnsureCashboxAsync(
        AppDbContext context,
        Branch branch,
        StoreBootstrapResult result,
        CancellationToken cancellationToken)
    {
        var existing = await context.Cashboxes
            .Where(x => !x.IsDeleted && x.IsActive && x.BranchId == branch.Id)
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is not null)
            return existing.Id;

        var candidate = await context.Cashboxes
            .Where(x => !x.IsDeleted && x.BranchId != branch.Id)
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (candidate is not null && !await CashboxHasDependentsAsync(context, candidate.Id, cancellationToken))
        {
            candidate.BranchId = branch.Id;
            if (!candidate.IsActive)
                candidate.IsActive = true;

            await context.SaveChangesAsync(cancellationToken);
            result.CashboxReparented = true;
            return candidate.Id;
        }

        if (candidate is not null)
            result.CashboxKeptForDependents = true;

        var created = new Cashbox
        {
            Name = DefaultCashboxName,
            BranchId = branch.Id,
            IsActive = true
        };

        context.Cashboxes.Add(created);
        await context.SaveChangesAsync(cancellationToken);
        result.CashboxCreated = true;

        return created.Id;
    }

    private static async Task<bool> WarehouseHasDependentsAsync(
        AppDbContext context,
        Guid warehouseId,
        CancellationToken cancellationToken)
    {
        return await context.ProductStocks
                   .AnyAsync(x => x.WarehouseId == warehouseId, cancellationToken)
            || await context.InventoryTransactions
                   .AnyAsync(x => x.WarehouseId == warehouseId, cancellationToken)
            || await context.StockAdjustments
                   .AnyAsync(x => x.WarehouseId == warehouseId, cancellationToken)
            || await context.StockTransfers
                   .AnyAsync(x => x.FromWarehouseId == warehouseId || x.ToWarehouseId == warehouseId, cancellationToken);
    }

    private static async Task<bool> CashboxHasDependentsAsync(
        AppDbContext context,
        Guid cashboxId,
        CancellationToken cancellationToken)
    {
        return await context.CashSessions
                   .AnyAsync(x => x.CashboxId == cashboxId, cancellationToken)
            || await context.CashTransactions
                   .AnyAsync(x => x.CashboxId == cashboxId, cancellationToken);
    }
}

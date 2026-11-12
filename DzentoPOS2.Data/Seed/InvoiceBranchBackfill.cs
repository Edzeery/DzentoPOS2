using DzentoPOS2.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DzentoPOS2.Data.Seed;

/// <summary>
/// Backfills empty BranchId on branch-scoped tables (invoices, expenses,
/// warehouses, cashboxes, quotations, snapshots) with the main branch ID.
/// Idempotent.
/// </summary>
public static class InvoiceBranchBackfill
{
    public static async Task SeedAsync(AppDbContext context, ILogger? logger = null)
    {
        var mainBranch = await context.Branches
            .Where(b => b.IsMainBranch && b.IsActive && !b.IsDeleted)
            .OrderBy(b => b.CreatedAt)
            .FirstOrDefaultAsync();

        if (mainBranch == null) return;

        int count = 0;

        // SalesInvoices
        count += await context.SalesInvoices
            .Where(s => s.BranchId == Guid.Empty)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.BranchId, mainBranch.Id));

        // PurchaseInvoices
        count += await context.PurchaseInvoices
            .Where(p => p.BranchId == Guid.Empty)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.BranchId, mainBranch.Id));

        // Expenses
        count += await context.Expenses
            .Where(e => e.BranchId == Guid.Empty)
            .ExecuteUpdateAsync(e => e.SetProperty(x => x.BranchId, mainBranch.Id));

        // Warehouses
        count += await context.Warehouses
            .Where(w => w.BranchId == Guid.Empty)
            .ExecuteUpdateAsync(w => w.SetProperty(x => x.BranchId, mainBranch.Id));

        // Cashboxes (cash sessions inherit the branch through their cashbox).
        count += await context.Cashboxes
            .Where(c => c.BranchId == Guid.Empty)
            .ExecuteUpdateAsync(c => c.SetProperty(x => x.BranchId, mainBranch.Id));

        // Cash registers
        count += await context.CashRegisters
            .Where(r => r.BranchId == Guid.Empty)
            .ExecuteUpdateAsync(r => r.SetProperty(x => x.BranchId, mainBranch.Id));

        // Sales quotations
        count += await context.SalesQuotations
            .Where(q => q.BranchId == Guid.Empty)
            .ExecuteUpdateAsync(q => q.SetProperty(x => x.BranchId, mainBranch.Id));

        // Legacy snapshots created before branch scoping. A legacy row can
        // share its SnapshotDate with a row that already belongs to the main
        // branch. Moving it would violate the unique (BranchId, SnapshotDate)
        // index, so those rows stay as legacy (NULL branch) aggregates instead
        // of being overwritten or deleted.
        var mainBranchSnapshotDates = await context.InventorySnapshots
            .Where(s => s.BranchId == mainBranch.Id)
            .Select(s => s.SnapshotDate)
            .ToListAsync();

        count += await context.InventorySnapshots
            .Where(s => (s.BranchId == null || s.BranchId == Guid.Empty)
                && !mainBranchSnapshotDates.Contains(s.SnapshotDate))
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.BranchId, mainBranch.Id));

        if (count > 0)
        {
            logger?.LogInformation("InvoiceBranchBackfill: updated {Count} rows", count);
        }
    }
}

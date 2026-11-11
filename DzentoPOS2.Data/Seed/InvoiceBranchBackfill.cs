using DzentoPOS2.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Seed;

/// <summary>
/// Backfills empty BranchId on SalesInvoices, PurchaseInvoices, and Expenses
/// with the main branch ID. Idempotent.
/// </summary>
public static class InvoiceBranchBackfill
{
    public static async Task SeedAsync(AppDbContext context)
    {
        var mainBranch = await context.Branches
            .Where(b => b.IsMainBranch && b.IsActive && !b.IsDeleted)
            .OrderBy(b => b.CreatedAt)
            .FirstOrDefaultAsync();

        if (mainBranch == null) return;

        int count = 0;

        // SalesInvoices
        var salesInvoices = await context.SalesInvoices
            .Where(s => s.BranchId == Guid.Empty)
            .ToListAsync();
        foreach (var inv in salesInvoices)
        {
            inv.BranchId = mainBranch.Id;
            count++;
        }

        // PurchaseInvoices
        var purchaseInvoices = await context.PurchaseInvoices
            .Where(p => p.BranchId == Guid.Empty)
            .ToListAsync();
        foreach (var inv in purchaseInvoices)
        {
            inv.BranchId = mainBranch.Id;
            count++;
        }

        // Expenses
        var expenses = await context.Expenses
            .Where(e => e.BranchId == Guid.Empty)
            .ToListAsync();
        foreach (var exp in expenses)
        {
            exp.BranchId = mainBranch.Id;
            count++;
        }

        if (count > 0)
        {
            await context.SaveChangesAsync();
        }
    }
}

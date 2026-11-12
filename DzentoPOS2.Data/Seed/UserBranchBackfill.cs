using DzentoPOS2.Core.Entities.Identity;
using DzentoPOS2.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DzentoPOS2.Data.Seed;

/// <summary>
/// Idempotent backfill: fills empty BranchId for all users with the main branch.
/// Logs the number of rows updated.
/// </summary>
public static class UserBranchBackfill
{
    public static async Task SeedAsync(AppDbContext context, ILogger? logger = null)
    {
        var mainBranch = await context.Branches
            .Where(b => b.IsMainBranch && b.IsActive && !b.IsDeleted)
            .OrderBy(b => b.CreatedAt)
            .FirstOrDefaultAsync();

        if (mainBranch == null) return;

        var usersWithoutBranch = await context.Users
            .Where(u => u.BranchId == null || u.BranchId == Guid.Empty)
            .ToListAsync();

        int count = 0;
        foreach (var user in usersWithoutBranch)
        {
            user.BranchId = mainBranch.Id;
            count++;
        }

        if (count > 0)
        {
            await context.SaveChangesAsync();
            logger?.LogInformation("UserBranchBackfill: updated {Count} users with main branch {BranchId}", count, mainBranch.Id);
        }
    }
}

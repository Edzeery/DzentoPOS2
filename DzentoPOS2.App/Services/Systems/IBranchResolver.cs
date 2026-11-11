using System;
using System.Threading;
using System.Threading.Tasks;
using DzentoPOS2.Core.Entities.Businesses;
using DzentoPOS2.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DzentoPOS2.App.Services.Systems;

/// <summary>
/// Stateless branch resolver. Resolves the active branch for a user:
/// 1. User's branch if it exists and is active (not deleted)
/// 2. Main branch (IsMainBranch DESC, CreatedAt ASC)
/// 3. First active branch
/// </summary>
public interface IBranchResolver
{
    Task<Branch?> ResolveAsync(Guid? preferred, CancellationToken ct = default);
}

public class BranchResolver : IBranchResolver
{
    private readonly IServiceProvider _serviceProvider;

    public BranchResolver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<Branch?> ResolveAsync(Guid? preferred, CancellationToken ct = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // 1. User's branch if exists and is active (not deleted)
        if (preferred.HasValue && preferred.Value != Guid.Empty)
        {
            var preferredBranch = await context.Branches
                .AsNoTracking()
                .Where(b => b.Id == preferred.Value && b.IsActive && !b.IsDeleted)
                .FirstOrDefaultAsync(ct);
            if (preferredBranch is not null)
                return preferredBranch;
        }

        // 2. Main branch
        var mainBranch = await context.Branches
            .AsNoTracking()
            .Where(b => b.IsActive && !b.IsDeleted)
            .OrderByDescending(b => b.IsMainBranch)
            .ThenBy(b => b.CreatedAt)
            .ThenBy(b => b.Id)
            .FirstOrDefaultAsync(ct);

        return mainBranch;
    }
}

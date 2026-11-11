using System;
using System.Threading;
using System.Threading.Tasks;
using DzentoPOS2.App.Authorization;
using DzentoPOS2.App.Session;
using DzentoPOS2.Core.Constants;
using DzentoPOS2.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DzentoPOS2.App.Services.Systems;

/// <summary>
/// Singleton implementation of IActiveBranchContext. Uses IBranchResolver to
/// resolve the active branch (no circular dependency). InitializeAsync is
/// called from AppApplicationContext after login.
/// </summary>
public class ActiveBranchContext : IActiveBranchContext
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IBranchResolver _branchResolver;
    private readonly object _gate = new();

    private Guid _activeBranchId;
    private string _activeBranchName = string.Empty;
    private bool _initialized;

    public ActiveBranchContext(IServiceProvider serviceProvider, IBranchResolver branchResolver)
    {
        _serviceProvider = serviceProvider;
        _branchResolver = branchResolver;
    }

    public Guid ActiveBranchId
    {
        get
        {
            if (!_initialized)
            {
                // Auto-initialize if user is available.
                using var scope = _serviceProvider.CreateScope();
                var currentUser = scope.ServiceProvider.GetRequiredService<ICurrentUserService>();
                if (currentUser.User is not null)
                {
                    InitializeAsync(currentUser).GetAwaiter().GetResult();
                }
                else
                {
                    throw new InvalidOperationException("ActiveBranchContext not initialized. Call InitializeAsync first.");
                }
            }
            return _activeBranchId;
        }
    }

    public string ActiveBranchName
    {
        get
        {
            if (!_initialized)
            {
                using var scope = _serviceProvider.CreateScope();
                var currentUser = scope.ServiceProvider.GetRequiredService<ICurrentUserService>();
                if (currentUser.User is not null)
                {
                    InitializeAsync(currentUser).GetAwaiter().GetResult();
                }
                else
                {
                    throw new InvalidOperationException("ActiveBranchContext not initialized. Call InitializeAsync first.");
                }
            }
            return _activeBranchName;
        }
    }

    public bool CanSwitchBranch
    {
        get
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
                return permissionService.HasPermissionAsync(Permissions.BranchesSwitch).GetAwaiter().GetResult();
            }
            catch
            {
                return false;
            }
        }
    }

    public event EventHandler<ActiveBranchChangedEventArgs>? BranchChanged;

    public async Task InitializeAsync(ICurrentUserService currentUser, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            if (_initialized) return;
        }

        var branch = await _branchResolver.ResolveAsync(currentUser.BranchId, cancellationToken);
        if (branch is null)
        {
            // Fallback: should not happen (resolver returns main branch as fallback)
            throw new InvalidOperationException("Could not resolve active branch.");
        }

        lock (_gate)
        {
            _activeBranchId = branch.Id;
            _activeBranchName = branch.Name;
            _initialized = true;
        }
    }

    public void Reset()
    {
        lock (_gate)
        {
            _activeBranchId = Guid.Empty;
            _activeBranchName = string.Empty;
            _initialized = false;
        }
    }

    public async Task<bool> TrySwitchBranchAsync(
        Guid branchId,
        CancellationToken cancellationToken = default)
    {
        if (!_initialized)
        {
            using var initScope = _serviceProvider.CreateScope();
            var currentUser = initScope.ServiceProvider.GetRequiredService<ICurrentUserService>();
            if (currentUser.User is not null)
                await InitializeAsync(currentUser, cancellationToken);
            else
                throw new InvalidOperationException("ActiveBranchContext not initialized. Call InitializeAsync first.");
        }

        // Permission check
        using var scope = _serviceProvider.CreateScope();
        var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
        bool hasPermission = await permissionService
            .HasPermissionAsync(Permissions.BranchesSwitch);
        if (!hasPermission)
            return false;

        // Validate branch
        using var dbScope = _serviceProvider.CreateScope();
        var context = dbScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var branch = await context.Branches
            .AsNoTracking()
            .Where(b => b.Id == branchId && b.IsActive && !b.IsDeleted)
            .Select(b => new { b.Id, b.Name })
            .FirstOrDefaultAsync(cancellationToken);

        if (branch is null)
            return false;

        lock (_gate)
        {
            if (_activeBranchId == branchId)
                return true;

            _activeBranchId = branch.Id;
            _activeBranchName = branch.Name;
        }

        BranchChanged?.Invoke(this, new ActiveBranchChangedEventArgs
        {
            BranchId = branch.Id,
            BranchName = branch.Name
        });

        return true;
    }
}

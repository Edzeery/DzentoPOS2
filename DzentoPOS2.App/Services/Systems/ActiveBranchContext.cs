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
/// Singleton implementation of IActiveBranchContext. Uses IServiceProvider to
/// resolve scoped services (ICurrentUserService, IPermissionService, AppDbContext)
/// on demand via CreateScope.
/// </summary>
public class ActiveBranchContext : IActiveBranchContext
{
    private readonly IServiceProvider _serviceProvider;
    private readonly object _gate = new();

    private Guid _activeBranchId;
    private string _activeBranchName = string.Empty;
    private bool _initialized;

    public ActiveBranchContext(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        // Subscribe to user changes for reset.
        var currentUser = serviceProvider.GetRequiredService<ICurrentUserService>();
        currentUser.UserChanged += (_, _) => Reset();
    }

    private void Reset()
    {
        lock (_gate)
        {
            _activeBranchId = Guid.Empty;
            _activeBranchName = string.Empty;
            _initialized = false;
        }
    }

    public Guid ActiveBranchId
    {
        get
        {
            EnsureInitialized();
            return _activeBranchId;
        }
    }

    public string ActiveBranchName
    {
        get
        {
            EnsureInitialized();
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

    private void EnsureInitialized()
    {
        lock (_gate)
        {
            if (_initialized) return;

            using var scope = _serviceProvider.CreateScope();
            var currentUser = scope.ServiceProvider.GetRequiredService<ICurrentUserService>();
            var user = currentUser.User;
            if (user is null)
                return; // Do not initialize if no user.

            _activeBranchId = user.BranchId ?? Guid.Empty;
            _activeBranchName = user.BranchName ?? string.Empty;
            _initialized = true;
        }
    }

    public async Task<bool> TrySwitchBranchAsync(
        Guid branchId,
        CancellationToken cancellationToken = default)
    {
        EnsureInitialized();

        // Permission check (resolve scoped service).
        using var scope = _serviceProvider.CreateScope();
        var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
        bool hasPermission = await permissionService
            .HasPermissionAsync(Permissions.BranchesSwitch);
        if (!hasPermission)
            return false;

        // Validate branch exists and is active.
        using var dbScope = _serviceProvider.CreateScope();
        var context = dbScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var branch = await context.Branches
            .AsNoTracking()
            .Where(b => b.Id == branchId && b.IsActive)
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

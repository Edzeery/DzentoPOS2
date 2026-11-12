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
    private bool _canSwitchBranch;

    public ActiveBranchContext(IServiceProvider serviceProvider, IBranchResolver branchResolver)
    {
        _serviceProvider = serviceProvider;
        _branchResolver = branchResolver;
    }

    public Guid ActiveBranchId
    {
        get
        {
            lock (_gate)
            {
                ThrowIfNotInitialized();
                return _activeBranchId;
            }
        }
    }

    public string ActiveBranchName
    {
        get
        {
            lock (_gate)
            {
                ThrowIfNotInitialized();
                return _activeBranchName;
            }
        }
    }

    public bool CanSwitchBranch
    {
        get
        {
            lock (_gate)
            {
                return _initialized && _canSwitchBranch;
            }
        }
    }

    private void ThrowIfNotInitialized()
    {
        if (!_initialized)
        {
            throw new InvalidOperationException(
                "ActiveBranchContext is not initialized. Call InitializeAsync after login before reading the active branch.");
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

        // Cache the switch permission once. The UI reads CanSwitchBranch
        // synchronously; it must never query the database from a getter.
        bool canSwitch;
        using (var scope = _serviceProvider.CreateScope())
        {
            var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
            canSwitch = await permissionService.HasPermissionAsync(Permissions.BranchesSwitch);
        }

        lock (_gate)
        {
            // A concurrent caller may have initialized while we were resolving.
            if (_initialized) return;

            if (branch is null)
            {
                // Keep the session usable: the shell shows a clear warning and
                // the admin is not locked out. No branch data is readable.
                SetEmptyLocked();
            }
            else
            {
                _activeBranchId = branch.Id;
                _activeBranchName = branch.Name;
                _canSwitchBranch = canSwitch;
                _initialized = true;
            }
        }
    }

    public void InitializeEmpty()
    {
        lock (_gate)
        {
            SetEmptyLocked();
        }
    }

    private void SetEmptyLocked()
    {
        _activeBranchId = Guid.Empty;
        _activeBranchName = string.Empty;
        _canSwitchBranch = false;
        _initialized = true;
    }

    public void Reset()
    {
        lock (_gate)
        {
            _activeBranchId = Guid.Empty;
            _activeBranchName = string.Empty;
            _canSwitchBranch = false;
            _initialized = false;
        }
    }

    public async Task<bool> TrySwitchBranchAsync(
        Guid branchId,
        CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            ThrowIfNotInitialized();
        }

        // Permission check (re-validated on every switch).
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
            _canSwitchBranch = true;
        }

        BranchChanged?.Invoke(this, new ActiveBranchChangedEventArgs
        {
            BranchId = branch.Id,
            BranchName = branch.Name
        });

        return true;
    }
}

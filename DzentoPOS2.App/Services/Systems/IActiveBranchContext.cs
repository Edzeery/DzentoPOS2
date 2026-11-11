using System;
using System.Threading;
using System.Threading.Tasks;
using DzentoPOS2.App.Session;

namespace DzentoPOS2.App.Services.Systems;

/// <summary>
/// Holds the currently active branch for the running session. Singleton.
/// The branch selector in MainForm sets it via TrySwitchBranchAsync.
/// </summary>
public interface IActiveBranchContext
{
    Guid ActiveBranchId { get; }
    string ActiveBranchName { get; }
    bool CanSwitchBranch { get; }

    event EventHandler<ActiveBranchChangedEventArgs>? BranchChanged;

    Task<bool> TrySwitchBranchAsync(Guid branchId, CancellationToken cancellationToken = default);

    Task InitializeAsync(ICurrentUserService currentUser, CancellationToken cancellationToken = default);
}

public class ActiveBranchChangedEventArgs : EventArgs
{
    public Guid BranchId { get; init; }
    public string BranchName { get; init; } = string.Empty;
}

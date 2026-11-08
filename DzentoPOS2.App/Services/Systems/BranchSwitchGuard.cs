using System;

namespace DzentoPOS2.App.Services.Systems;

/// <summary>
/// Singleton guard that tracks whether the current UI allows a branch switch.
/// Pages with a non-empty cart set CanSwitchBranch = false.
/// </summary>
public class BranchSwitchGuard : IBranchSwitchGuard
{
    private bool _canSwitch = true;

    public bool CanSwitchBranch
    {
        get => _canSwitch;
        set
        {
            if (_canSwitch == value) return;
            _canSwitch = value;
            CanSwitchChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? CanSwitchChanged;
}

using System;

namespace DzentoPOS2.App.Services.Systems;

/// <summary>
/// Default page-level guard. Pages set CanSwitchBranch based on their state.
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

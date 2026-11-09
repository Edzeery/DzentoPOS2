using System;
using System.Collections.Generic;

namespace DzentoPOS2.App.Services.Systems;

/// <summary>
/// Registry of UI-level guards that can veto a branch switch.
/// Pages register/unregister their guard on load/unload.
/// </summary>
public interface IBranchSwitchGuardRegistry
{
    void Register(IBranchSwitchGuard guard);
    void Unregister(IBranchSwitchGuard guard);
    bool CanSwitch { get; }
    event EventHandler? CanSwitchChanged;
}

/// <summary>
/// A page-level guard that reports whether it allows a branch switch.
/// </summary>
public interface IBranchSwitchGuard
{
    bool CanSwitchBranch { get; set; }
    event EventHandler? CanSwitchChanged;
}

public class BranchSwitchGuardRegistry : IBranchSwitchGuardRegistry
{
    private readonly List<IBranchSwitchGuard> _guards = new();
    private readonly object _gate = new();

    public bool CanSwitch
    {
        get
        {
            lock (_gate)
            {
                return _guards.All(g => g.CanSwitchBranch);
            }
        }
    }

    public event EventHandler? CanSwitchChanged;

    public void Register(IBranchSwitchGuard guard)
    {
        lock (_gate)
        {
            if (!_guards.Contains(guard))
                _guards.Add(guard);
        }
        guard.CanSwitchChanged += OnGuardChanged;
        CanSwitchChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Unregister(IBranchSwitchGuard guard)
    {
        lock (_gate)
        {
            _guards.Remove(guard);
        }
        guard.CanSwitchChanged -= OnGuardChanged;
        CanSwitchChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnGuardChanged(object? sender, EventArgs e)
    {
        CanSwitchChanged?.Invoke(this, EventArgs.Empty);
    }
}

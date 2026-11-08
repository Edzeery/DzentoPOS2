using System;

namespace DzentoPOS2.App.Services.Systems;

/// <summary>
/// UI-level guard that reports whether the current page allows a branch switch.
/// Pages with a non-empty cart register a guard that returns false.
/// </summary>
public interface IBranchSwitchGuard
{
    bool CanSwitchBranch { get; set; }
    event EventHandler? CanSwitchChanged;
}

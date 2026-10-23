using DzentoPOS2.WinForms.Models;

namespace DzentoPOS2.WinForms.Events;

public sealed class BranchChangedEventArgs : EventArgs
{
    public BranchChangedEventArgs(BranchViewModel branch)
    {
        Branch = branch;
    }

    public BranchViewModel Branch { get; }
}

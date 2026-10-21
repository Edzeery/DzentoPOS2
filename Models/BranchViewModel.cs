namespace DzentoPOS2.WinForms.Models;

public sealed class BranchViewModel
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Address { get; init; }

    public bool IsMainBranch { get; init; }

    public bool IsActive { get; init; }

    public override string ToString() => Name;
}
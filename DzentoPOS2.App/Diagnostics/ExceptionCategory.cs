namespace DzentoPOS2.App.Diagnostics;

/// <summary>
/// Coarse classification of an exception, used to pick a friendly, localized
/// message instead of leaking a raw provider error to the cashier.
/// </summary>
public enum ExceptionCategory
{
    Unknown,
    ForeignKey,
    UniqueConstraint,
    NotNull,
    Concurrency,
    Domain
}

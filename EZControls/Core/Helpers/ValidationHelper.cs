namespace EZControls.Core.Helpers;


// ══════════════════════════════════════════════════════════════════════════════
//  ValidationHelper
// ══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Lightweight validation helpers for common input constraints.
/// All methods are pure functions (no side effects, no allocations).
/// </summary>
public static class ValidationHelper
{
    /// <summary>Returns <see langword="true"/> when <paramref name="text"/> is non-null and non-whitespace.</summary>
    public static bool IsRequired(string? text) =>
        !string.IsNullOrWhiteSpace(text);

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="text"/> looks like a valid
    /// e-mail address (contains exactly one '@' with a '.' after it).
    /// This is a fast heuristic, not an RFC 5322 validator.
    /// </summary>
    public static bool IsEmail(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;

        int atIndex = text.IndexOf('@');
        if (atIndex <= 0 || atIndex == text.Length - 1) return false;

        int dotIndex = text.IndexOf('.', atIndex);
        return dotIndex > atIndex + 1 && dotIndex < text.Length - 1;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="text"/> has at least <paramref name="length"/> characters.</summary>
    public static bool MinLength(string? text, int length) =>
        (text?.Length ?? 0) >= length;

    /// <summary>Returns <see langword="true"/> when <paramref name="text"/> represents a parseable integer.</summary>
    public static bool IsNumeric(string? text) =>
        long.TryParse(text, out _);
}

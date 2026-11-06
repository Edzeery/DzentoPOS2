namespace DzentoPOS2.Shared.Helpers;

/// <summary>
/// The localized short unit suffixes used by
/// <see cref="KpiValueFormatter"/> when it shortens a money value
/// (thousand / million / billion).
///
/// It lives next to the formatter so the formatter itself stays free of any
/// resource or UI dependency; the WinForms layer builds one of these from
/// <c>AppStrings</c> and passes it in.
/// </summary>
/// <param name="Thousand">Suffix appended to values scaled by 1 000.</param>
/// <param name="Million">Suffix appended to values scaled by 1 000 000.</param>
/// <param name="Billion">Suffix appended to values scaled by 1 000 000 000.</param>
public sealed record KpiCompactSuffixes(
    string Thousand,
    string Million,
    string Billion)
{
    /// <summary>
    /// Culture-neutral fallback (the invariant abbreviations). Callers that
    /// render for the user should pass the localized suffixes instead.
    /// </summary>
    public static KpiCompactSuffixes Invariant { get; } = new("K", "M", "B");

    /// <summary>Returns the suffix for the 0-based unit (0 = thousand).</summary>
    public string ForUnit(int unit) => unit switch
    {
        0 => Thousand,
        1 => Million,
        _ => Billion
    };
}

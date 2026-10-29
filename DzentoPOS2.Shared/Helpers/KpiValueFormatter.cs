using System.Globalization;

namespace DzentoPOS2.Shared.Helpers;

/// <summary>How a KPI number should be rendered.</summary>
public enum KpiValueKind
{
    /// <summary>Whole count: grouping separators, no decimals.</summary>
    Count,

    /// <summary>Money amount: fixed currency digits, no symbol by default.</summary>
    Money,

    /// <summary>Money amount shortened to a localized thousand / million / billion unit.</summary>
    CompactMoney,

    /// <summary>Percentage, e.g. 12.5%.</summary>
    Percent
}

/// <summary>
/// Pure, WinForms-free formatting of KPI numbers. It is the single place that
/// turns a numeric value into a display string, always using an explicit
/// <see cref="CultureInfo"/> (never a hardcoded culture) so the same value is
/// rendered identically on screen, in lists and in the receipt.
///
/// The compact money suffixes are not hardcoded: they are supplied through
/// <see cref="KpiCompactSuffixes"/> (built from <c>AppStrings</c> by the UI) and
/// fall back to <see cref="KpiCompactSuffixes.Invariant"/> only when none is
/// given.
/// </summary>
public static class KpiValueFormatter
{
    private static readonly decimal[] CompactDivisors =
    {
        1_000m,
        1_000_000m,
        1_000_000_000m
    };

    public static string Format(
        decimal value,
        KpiValueKind kind,
        CultureInfo? culture = null,
        int currencyDigits = 2,
        bool includeCurrencySymbol = false,
        string? currencySymbol = null,
        KpiCompactSuffixes? compactSuffixes = null)
    {
        culture ??= CultureInfo.CurrentCulture;

        var digits = Math.Clamp(currencyDigits, 0, 4);

        var text = kind switch
        {
            KpiValueKind.Count => value.ToString("#,##0", culture),
            KpiValueKind.Money => value.ToString("N" + digits, culture),
            KpiValueKind.CompactMoney => Compact(
                value, culture, digits, compactSuffixes ?? KpiCompactSuffixes.Invariant),
            KpiValueKind.Percent => value.ToString("0.#", culture) + "%",
            _ => value.ToString(culture)
        };

        if (kind == KpiValueKind.Money && includeCurrencySymbol && !string.IsNullOrWhiteSpace(currencySymbol))
            text = text + " " + currencySymbol;

        return text;
    }

    private static string Compact(
        decimal value,
        CultureInfo culture,
        int digits,
        KpiCompactSuffixes suffixes)
    {
        var abs = Math.Abs(value);

        if (abs < 1_000m)
            return value.ToString("N" + digits, culture);

        var unit = abs >= CompactDivisors[2] ? 2 : abs >= CompactDivisors[1] ? 1 : 0;
        var scaled = value / CompactDivisors[unit];

        // A value that would round up to "1000<unit>" is promoted to the next
        // unit (e.g. 999 999 -> 1M instead of 1000K).
        if (unit < CompactDivisors.Length - 1 && Math.Abs(Math.Round(scaled, 2)) >= 1000m)
        {
            unit++;
            scaled = value / CompactDivisors[unit];
        }

        return scaled.ToString("0.##", culture) + suffixes.ForUnit(unit);
    }
}

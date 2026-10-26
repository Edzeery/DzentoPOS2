using System.Globalization;

namespace DzentoPOS2.Shared.Helpers;

/// <summary>How a KPI number should be rendered.</summary>
public enum KpiValueKind
{
    /// <summary>Whole count: grouping separators, no decimals.</summary>
    Count,

    /// <summary>Money amount: fixed currency digits, no symbol by default.</summary>
    Money,

    /// <summary>Money amount shortened to K / M / B.</summary>
    CompactMoney,

    /// <summary>Percentage, e.g. 12.5%.</summary>
    Percent
}

/// <summary>
/// Pure, WinForms-free formatting of KPI numbers. It is the single place that
/// turns a numeric value into a display string, always using an explicit
/// <see cref="CultureInfo"/> (never a hardcoded culture) so the same value is
/// rendered identically on screen, in lists and in the receipt.
/// </summary>
public static class KpiValueFormatter
{
    public static string Format(
        decimal value,
        KpiValueKind kind,
        CultureInfo? culture = null,
        int currencyDigits = 2,
        bool includeCurrencySymbol = false,
        string? currencySymbol = null)
    {
        culture ??= CultureInfo.CurrentCulture;

        var digits = Math.Clamp(currencyDigits, 0, 4);

        var text = kind switch
        {
            KpiValueKind.Count => value.ToString("#,##0", culture),
            KpiValueKind.Money => value.ToString("N" + digits, culture),
            KpiValueKind.CompactMoney => Compact(value, culture, digits),
            KpiValueKind.Percent => value.ToString("0.#", culture) + "%",
            _ => value.ToString(culture)
        };

        if (kind == KpiValueKind.Money && includeCurrencySymbol && !string.IsNullOrWhiteSpace(currencySymbol))
            text = text + " " + currencySymbol;

        return text;
    }

    private static string Compact(decimal value, CultureInfo culture, int digits)
    {
        var abs = Math.Abs(value);

        if (abs < 1_000m)
            return value.ToString("N" + digits, culture);

        string suffix;
        decimal scaled;

        if (abs >= 1_000_000_000m)
        {
            scaled = value / 1_000_000_000m;
            suffix = "B";
        }
        else if (abs >= 1_000_000m)
        {
            scaled = value / 1_000_000m;
            suffix = "M";
        }
        else
        {
            scaled = value / 1_000m;
            suffix = "K";
        }

        // A value that would round up to "1000K" is promoted to the next unit.
        if (Math.Abs(Math.Round(scaled, 2)) >= 1000m)
        {
            if (suffix == "K")
            {
                scaled = value / 1_000_000m;
                suffix = "M";
            }
            else if (suffix == "M")
            {
                scaled = value / 1_000_000_000m;
                suffix = "B";
            }
        }

        return scaled.ToString("0.##", culture) + suffix;
    }
}
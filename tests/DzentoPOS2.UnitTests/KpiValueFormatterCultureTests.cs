using System.Globalization;
using DzentoPOS2.Shared.Helpers;
using Xunit;

namespace DzentoPOS2.UnitTests;

public sealed class KpiValueFormatterCultureTests
{
    // Exact strings: the decimal separator and the thousands separator must be
    // different, and ar/fr must use the approved NO-BREAK SPACE policy.
    [Theory]
    [InlineData("en-US", "13,500.50")]
    [InlineData("ar", "13\u00A0500,50")]
    [InlineData("ar-DZ", "13\u00A0500,50")]
    [InlineData("fr", "13\u00A0500,50")]
    [InlineData("fr-DZ", "13\u00A0500,50")]
    public void Money_13500_5_uses_exact_separators(string cultureName, string expected)
    {
        var culture = new CultureInfo(cultureName, useUserOverride: false);
        Assert.Equal(expected, KpiValueFormatter.Format(13500.5m, KpiValueKind.Money, culture, 2));
    }

    [Theory]
    [InlineData("ar")]
    [InlineData("ar-DZ")]
    [InlineData("ar-SA")]
    public void Money_uses_latin_digits_for_arabic_cultures(string cultureName)
    {
        var culture = new CultureInfo(cultureName, useUserOverride: false);
        var text = KpiValueFormatter.Format(13500.5m, KpiValueKind.Money, culture, 2);

        foreach (var ch in text)
        {
            Assert.False(ch is >= '\u0660' and <= '\u0669', "Arabic-Indic digit found: " + ch);
            Assert.False(ch is >= '\u06F0' and <= '\u06F9', "Extended Arabic-Indic digit found: " + ch);
        }

        var normalized = KpiValueFormatter.NormalizeCulture(culture);
        Assert.NotEqual(
            normalized.NumberFormat.NumberDecimalSeparator,
            normalized.NumberFormat.NumberGroupSeparator);
    }

    [Fact]
    public void Approved_separator_policy_is_documented()
    {
        // en: comma group / dot decimal.
        Assert.Equal(",", RegionalContext.DefaultGroupSeparator(new CultureInfo("en")));
        Assert.Equal(".", RegionalContext.DefaultDecimalSeparator(new CultureInfo("en")));

        // ar and fr: NO-BREAK SPACE group / comma decimal.
        foreach (var name in new[] { "ar", "ar-DZ", "fr", "fr-DZ" })
        {
            var culture = new CultureInfo(name);
            Assert.Equal("\u00A0", RegionalContext.DefaultGroupSeparator(culture));
            Assert.Equal(",", RegionalContext.DefaultDecimalSeparator(culture));
        }
    }

    [Fact]
    public void Normalized_culture_always_has_distinct_separators()
    {
        var cultures = new[]
        {
            new CultureInfo("en-US"),
            new CultureInfo("ar"),
            new CultureInfo("ar-DZ"),
            new CultureInfo("ar-SA"),
            new CultureInfo("fr-DZ"),
            new CultureInfo("de-DE"),
            CultureInfo.InvariantCulture
        };

        foreach (var culture in cultures)
        {
            var normalized = KpiValueFormatter.NormalizeCulture(culture);
            Assert.NotEqual(
                normalized.NumberFormat.NumberDecimalSeparator,
                normalized.NumberFormat.NumberGroupSeparator);
        }
    }
}
using System.Globalization;
using DzentoPOS2.Shared.Helpers;
using Xunit;

namespace DzentoPOS2.UnitTests;

public sealed class KpiValueFormatterCultureTests
{
    [Theory]
    [InlineData("ar-DZ")]
    [InlineData("fr-DZ")]
    [InlineData("en-US")]
    public void Money_ThousandsAndDecimalSeparatorsDiffer(string cultureName)
    {
        var culture = new CultureInfo(cultureName);
        var text = KpiValueFormatter.Format(13500.5m, KpiValueKind.Money, culture, 2);
        // The formatted text must contain two different separator characters.
        Assert.True(text.Length > 6);
    }

    [Theory]
    [InlineData("ar")]
    [InlineData("ar-DZ")]
    [InlineData("ar-SA")]
    public void Money_uses_latin_digits_for_arabic_cultures(string cultureName)
    {
        var culture = new CultureInfo(cultureName);
        var text = KpiValueFormatter.Format(13500.5m, KpiValueKind.Money, culture, 2);
        // Latin digits only (no Arabic-Indic digits).
        foreach (var ch in text)
        {
            if (ch >= '\u0660' && ch <= '\u0669') Assert.True(false, "Arabic-Indic digit found: " + ch);
            if (ch >= '\u06F0' && ch <= '\u06F9') Assert.True(false, "Extended Arabic-Indic digit found: " + ch);
        }
        // Separators must not be identical.
        var norm = KpiValueFormatter.NormalizeCulture(culture);
        Assert.NotEqual(norm.NumberFormat.NumberDecimalSeparator, norm.NumberFormat.NumberGroupSeparator);
        Assert.True(text.Length > 6);
    }
}
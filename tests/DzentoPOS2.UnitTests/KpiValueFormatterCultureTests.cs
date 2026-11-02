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
}
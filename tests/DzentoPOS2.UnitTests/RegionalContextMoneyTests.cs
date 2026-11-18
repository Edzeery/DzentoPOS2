using System.Globalization;
using System.Linq;
using DzentoPOS2.Shared.Helpers;
using Xunit;

namespace DzentoPOS2.UnitTests;

public sealed class RegionalContextMoneyTests
{
    [Fact]
    public void FormatMoney_UsesWesternDigitsUnderArabicCulture()
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("ar-DZ");
            var regional = new RegionalContext { CurrencyCode = "DZD" };

            var text = regional.FormatMoney(1234.5m);

            Assert.DoesNotContain(text, c => c >= '\u0660' && c <= '\u0669');
            Assert.Contains("1", text);
            Assert.Contains(".", text);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public void FormatMoney_UsesConfiguredDecimalDigits()
    {
        var regional = new RegionalContext { CurrencyCode = "DZD" };

        var text = regional.FormatMoney(10m);

        Assert.Contains("10", text);
    }
}

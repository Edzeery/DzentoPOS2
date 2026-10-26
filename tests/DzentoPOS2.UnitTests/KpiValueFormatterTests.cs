using System.Globalization;
using DzentoPOS2.Shared.Helpers;
using Xunit;

namespace DzentoPOS2.UnitTests;

public sealed class KpiValueFormatterTests
{
    private static readonly CultureInfo EnUs = new("en-US", useUserOverride: false);
    private static readonly CultureInfo Ar = new("ar", useUserOverride: false);
    private static readonly CultureInfo ArDz = new("ar-DZ", useUserOverride: false);
    private static readonly CultureInfo FrFr = new("fr-FR", useUserOverride: false);
    private static readonly CultureInfo FrDz = new("fr-DZ", useUserOverride: false);

    public static IEnumerable<object[]> Cultures =>
    [
        [EnUs],
        [Ar],
        [ArDz],
        [FrFr],
        [FrDz]
    ];

    private static string GroupSeparator(CultureInfo culture) => culture.NumberFormat.NumberGroupSeparator;

    [Theory]
    [MemberData(nameof(Cultures))]
    public void Counts_use_grouping_only_from_one_thousand(CultureInfo culture)
    {
        foreach (var value in new decimal[] { 0, 1, 2, 6, 1100, 22000 })
        {
            var text = KpiValueFormatter.Format(value, KpiValueKind.Count, culture);
            var sep = GroupSeparator(culture);

            Assert.Equal(value.ToString("#,##0", culture), text);

            // No duplicated groups (the old bug produced "44,44,891").
            Assert.DoesNotContain(sep + sep, text);

            if (value < 1000)
                Assert.DoesNotContain(sep, text);
        }
    }

    [Fact]
    public void Counts_exact_strings_in_en_us()
    {
        Assert.Equal("0", KpiValueFormatter.Format(0, KpiValueKind.Count, EnUs));
        Assert.Equal("1", KpiValueFormatter.Format(1, KpiValueKind.Count, EnUs));
        Assert.Equal("2", KpiValueFormatter.Format(2, KpiValueKind.Count, EnUs));
        Assert.Equal("6", KpiValueFormatter.Format(6, KpiValueKind.Count, EnUs));
        Assert.Equal("1,100", KpiValueFormatter.Format(1100, KpiValueKind.Count, EnUs));
        Assert.Equal("22,000", KpiValueFormatter.Format(22000, KpiValueKind.Count, EnUs));
    }

    [Theory]
    [MemberData(nameof(Cultures))]
    public void Money_uses_currency_digits_and_culture_separators(CultureInfo culture)
    {
        foreach (var value in new decimal[] { 5891, 44891, 1818450, 3131500, 0.5m, -44891 })
        {
            var text = KpiValueFormatter.Format(value, KpiValueKind.Money, culture, 2);

            Assert.Equal(value.ToString("N2", culture), text);
            Assert.DoesNotContain(GroupSeparator(culture) + GroupSeparator(culture), text);
        }
    }

    [Fact]
    public void Money_exact_strings_in_en_us()
    {
        Assert.Equal("5,891.00", KpiValueFormatter.Format(5891, KpiValueKind.Money, EnUs, 2));
        Assert.Equal("44,891.00", KpiValueFormatter.Format(44891, KpiValueKind.Money, EnUs, 2));
        Assert.Equal("1,818,450.00", KpiValueFormatter.Format(1818450, KpiValueKind.Money, EnUs, 2));
        Assert.Equal("3,131,500.00", KpiValueFormatter.Format(3131500, KpiValueKind.Money, EnUs, 2));
        Assert.Equal("0.50", KpiValueFormatter.Format(0.5m, KpiValueKind.Money, EnUs, 2));
        Assert.Equal("-44,891.00", KpiValueFormatter.Format(-44891, KpiValueKind.Money, EnUs, 2));
    }

    [Fact]
    public void Money_honours_currency_digits()
    {
        Assert.Equal("1,234", KpiValueFormatter.Format(1234, KpiValueKind.Money, EnUs, 0));
        Assert.Equal("1,234.500", KpiValueFormatter.Format(1234.5m, KpiValueKind.Money, EnUs, 3));
    }

    [Fact]
    public void Compact_money_uses_units_and_culture_separators()
    {
        Assert.Equal("1.25M", KpiValueFormatter.Format(1_250_000m, KpiValueKind.CompactMoney, EnUs, 2));
        Assert.Equal("1M", KpiValueFormatter.Format(999_999m, KpiValueKind.CompactMoney, EnUs, 2));
        Assert.Equal("125K", KpiValueFormatter.Format(125_000m, KpiValueKind.CompactMoney, EnUs, 2));
        Assert.Equal("1.5B", KpiValueFormatter.Format(1_500_000_000m, KpiValueKind.CompactMoney, EnUs, 2));

        // fr-FR uses a comma as the decimal separator.
        Assert.Equal("1,25M", KpiValueFormatter.Format(1_250_000m, KpiValueKind.CompactMoney, FrFr, 2));
    }

    [Fact]
    public void Percent_is_formatted_with_the_culture()
    {
        Assert.Equal("12.5%", KpiValueFormatter.Format(12.5m, KpiValueKind.Percent, EnUs));
        Assert.Equal("12,5%", KpiValueFormatter.Format(12.5m, KpiValueKind.Percent, FrFr));
    }

    [Fact]
    public void Never_a_hardcoded_culture_defaults_to_current()
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = FrFr;
            Assert.Equal(1234.ToString("#,##0", FrFr), KpiValueFormatter.Format(1234, KpiValueKind.Count));
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void Regression_no_duplicated_group_for_44891()
    {
        // The old card produced "44,44,891" for this value.
        var text = KpiValueFormatter.Format(44891, KpiValueKind.Money, EnUs, 2);

        Assert.Equal("44,891.00", text);
        Assert.DoesNotContain("44,44", text);
    }

    [Fact]
    public void Regression_count_of_two_has_no_decimal_or_group()
    {
        // The old card produced "2,200" (N2 -> "2,00" stripped to "200").
        Assert.Equal("2", KpiValueFormatter.Format(2, KpiValueKind.Count, Ar));
        Assert.Equal("2", KpiValueFormatter.Format(2, KpiValueKind.Count, ArDz));
        Assert.NotEqual("2,200", KpiValueFormatter.Format(2, KpiValueKind.Count, Ar));
    }
}
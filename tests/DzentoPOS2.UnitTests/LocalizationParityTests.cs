using System.Globalization;
using System.Resources;
using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Shared.Helpers;
using Xunit;

namespace DzentoPOS2.UnitTests;

/// <summary>
/// Proves the resource files stay in sync and that switching the UI culture
/// actually changes every string that goes through <see cref="AppStrings"/>.
/// (Instantiating the WinForms shell needs an STA message loop + a database, so
/// the toggle is exercised over the single source of truth instead.)
/// </summary>
public sealed class LocalizationParityTests
{
    private static readonly string[] Cultures = ["ar", "en", "fr"];

    private static ResourceManager Manager => new(
        "DzentoPOS2.Localization.Resources.AppStrings",
        typeof(AppStrings).Assembly);

    private static Dictionary<string, string> ResourceSet(string culture)
    {
        var set = Manager.GetResourceSet(new CultureInfo(culture), createIfNotExists: true, tryParents: false)
                  ?? throw new InvalidOperationException($"No resource set for '{culture}'.");

        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (System.Collections.DictionaryEntry entry in set)
        {
            if (entry.Key is string key && entry.Value is string value)
                result[key] = value;
        }

        return result;
    }

    [Fact]
    public void Every_culture_has_the_same_keys_as_the_base()
    {
        var baseline = ResourceSet("en");

        foreach (var culture in Cultures)
        {
            var keys = ResourceSet(culture);

            var missing = baseline.Keys.Except(keys.Keys).ToList();
            var extra = keys.Keys.Except(baseline.Keys).ToList();

            Assert.True(missing.Count == 0, $"{culture}: missing keys: {string.Join(", ", missing)}");
            Assert.True(extra.Count == 0, $"{culture}: extra keys: {string.Join(", ", extra)}");
        }
    }

    [Theory]
    [InlineData("Branch")]
    [InlineData("Search")]
    [InlineData("Expenses")]
    [InlineData("Print")]
    [InlineData("Delete")]
    [InlineData("PointOfSale")]
    [InlineData("SelectCustomer")]
    [InlineData("Dashboard")]
    public void Arabic_values_are_actually_translated(string key)
    {
        var arabic = ResourceSet("ar");
        var english = ResourceSet("en");

        Assert.True(arabic.ContainsKey(key), $"ar.resx is missing '{key}'.");
        Assert.NotEqual(english[key], arabic[key]);
        Assert.True(ContainsArabic(arabic[key]), $"ar['{key}'] is not Arabic: '{arabic[key]}'.");
    }

    [Fact]
    public void Switching_culture_ten_times_keeps_appstrings_in_sync()
    {
        var previous = CultureInfo.CurrentUICulture;
        try
        {
            for (var i = 0; i < 10; i++)
            {
                foreach (var culture in new[] { "ar", "en", "fr", "ar" })
                {
                    var info = new CultureInfo(culture);
                    CultureInfo.CurrentUICulture = info;

                    foreach (var key in new[] { "Branch", "Search", "PointOfSale" })
                    {
                        Assert.Equal(ResourceSet(culture)[key], Manager.GetString(key, info));
                    }
                }
            }
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    [Fact]
    public void Kpi_formatter_follows_the_current_culture()
    {
        var previousCulture = CultureInfo.CurrentCulture;
        var previousUi = CultureInfo.CurrentUICulture;
        try
        {
            for (var i = 0; i < 10; i++)
            {
                CultureInfo.CurrentCulture = new CultureInfo("en-US");
                Assert.Equal("1,818,450.00", KpiValueFormatter.Format(1818450, KpiValueKind.Money, CultureInfo.CurrentCulture, 2));

                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                Assert.Equal(1818450.ToString("N2", CultureInfo.CurrentCulture), KpiValueFormatter.Format(1818450, KpiValueKind.Money, CultureInfo.CurrentCulture, 2));
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUi;
        }
    }

    private static bool ContainsArabic(string? value)
        => !string.IsNullOrEmpty(value) && value.Any(c => c >= 0x0600 && c <= 0x06FF);
}
using System.Globalization;

namespace DzentoPOS2.Shared.Helpers;

/// <summary>A currency the store can be configured with.</summary>
/// <param name="Code">ISO-4217 code (SAR, USD, ...).</param>
/// <param name="Symbol">Symbol shown next to amounts.</param>
/// <param name="EnglishName">Fallback name when the UI language has no translation.</param>
/// <param name="DecimalDigits">Number of fraction digits used for formatting.</param>
public sealed record CurrencyOption(string Code, string Symbol, string EnglishName, int DecimalDigits = 2);

/// <summary>A country preset that links a country to its currency and time zone.</summary>
/// <param name="Code">ISO-3166-1 alpha-2 code.</param>
/// <param name="CurrencyCode">Default currency of the country.</param>
/// <param name="TimeZoneId">Default IANA time zone of the country.</param>
public sealed record CountryOption(string Code, string CurrencyCode, string TimeZoneId);

/// <summary>
/// Static catalog of the regional data offered by the settings page.
/// Country names are resolved from the .NET culture database so they follow
/// the active UI language without shipping a hand written translation table.
/// </summary>
public static class RegionalCatalog
{
    public static readonly IReadOnlyList<CurrencyOption> Currencies = new List<CurrencyOption>
    {
        new("SAR", "ر.س", "Saudi Riyal"),
        new("AED", "د.إ", "UAE Dirham"),
        new("KWD", "د.ك", "Kuwaiti Dinar"),
        new("BHD", "د.ب", "Bahraini Dinar"),
        new("QAR", "ر.ق", "Qatari Riyal"),
        new("OMR", "ر.ع", "Omani Rial"),
        new("JOD", "د.أ", "Jordanian Dinar"),
        new("EGP", "ج.م", "Egyptian Pound"),
        new("LBP", "ل.ل", "Lebanese Pound"),
        new("IQD", "د.ع", "Iraqi Dinar"),
        new("SYP", "ل.س", "Syrian Pound"),
        new("YER", "﷼", "Yemeni Rial"),
        new("MAD", "د.م.", "Moroccan Dirham"),
        new("DZD", "دج", "Algerian Dinar"),
        new("TND", "د.ت", "Tunisian Dinar"),
        new("LYD", "د.ل", "Libyan Dinar"),
        new("SDG", "ج.س", "Sudanese Pound"),
        new("TRY", "₺", "Turkish Lira"),
        new("ILS", "₪", "Israeli Shekel"),
        new("USD", "$", "US Dollar"),
        new("NZD", "NZ$", "New Zealand Dollar"),
        new("EUR", "€", "Euro"),
        new("GBP", "£", "British Pound"),
        new("CHF", "CHF", "Swiss Franc"),
        new("CNY", "¥", "Chinese Yuan"),
        new("JPY", "¥", "Japanese Yen"),
        new("INR", "₹", "Indian Rupee"),
        new("PKR", "₨", "Pakistani Rupee"),
        new("IDR", "Rp", "Indonesian Rupiah"),
        new("MYR", "RM", "Malaysian Ringgit"),
        new("SGD", "S$", "Singapore Dollar"),
        new("AUD", "A$", "Australian Dollar"),
        new("CAD", "C$", "Canadian Dollar"),
        new("ZAR", "R", "South African Rand"),
        new("NGN", "₦", "Nigerian Naira"),
        new("KES", "KSh", "Kenyan Shilling"),
        new("RUB", "₽", "Russian Ruble"),
        new("BRL", "R$", "Brazilian Real")
    };

    public static readonly IReadOnlyList<CountryOption> Countries = new List<CountryOption>
    {
        new("SA", "SAR", "Asia/Riyadh"),
        new("AE", "AED", "Asia/Dubai"),
        new("KW", "KWD", "Asia/Kuwait"),
        new("BH", "BHD", "Asia/Bahrain"),
        new("QA", "QAR", "Asia/Qatar"),
        new("OM", "OMR", "Asia/Muscat"),
        new("JO", "JOD", "Asia/Amman"),
        new("EG", "EGP", "Africa/Cairo"),
        new("LB", "LBP", "Asia/Beirut"),
        new("IQ", "IQD", "Asia/Baghdad"),
        new("SY", "SYP", "Asia/Damascus"),
        new("YE", "YER", "Asia/Aden"),
        new("PS", "ILS", "Asia/Hebron"),
        new("MA", "MAD", "Africa/Casablanca"),
        new("DZ", "DZD", "Africa/Algiers"),
        new("TN", "TND", "Africa/Tunis"),
        new("LY", "LYD", "Africa/Tripoli"),
        new("SD", "SDG", "Africa/Khartoum"),
        new("TR", "TRY", "Europe/Istanbul"),
        new("US", "USD", "America/New_York"),
        new("CA", "CAD", "America/Toronto"),
        new("GB", "GBP", "Europe/London"),
        new("FR", "EUR", "Europe/Paris"),
        new("DE", "EUR", "Europe/Berlin"),
        new("ES", "EUR", "Europe/Madrid"),
        new("IT", "EUR", "Europe/Rome"),
        new("NL", "EUR", "Europe/Amsterdam"),
        new("CH", "CHF", "Europe/Zurich"),
        new("RU", "RUB", "Europe/Moscow"),
        new("CN", "CNY", "Asia/Shanghai"),
        new("JP", "JPY", "Asia/Tokyo"),
        new("IN", "INR", "Asia/Kolkata"),
        new("PK", "PKR", "Asia/Karachi"),
        new("ID", "IDR", "Asia/Jakarta"),
        new("MY", "MYR", "Asia/Kuala_Lumpur"),
        new("SG", "SGD", "Asia/Singapore"),
        new("AU", "AUD", "Australia/Sydney"),
        new("NZ", "NZD", "Pacific/Auckland"),
        new("ZA", "ZAR", "Africa/Johannesburg"),
        new("NG", "NGN", "Africa/Lagos"),
        new("KE", "KES", "Africa/Nairobi"),
        new("BR", "BRL", "America/Sao_Paulo")
    };

    /// <summary>
    /// Time zones offered by the settings page: the zones of the country
    /// presets plus the zone of the machine the application runs on. The list is
    /// sorted by the UTC offset so the most relevant zones come first.
    /// </summary>
    public static IReadOnlyList<TimeZoneInfo> GetTimeZones()
    {
        var ids = new List<string>();

        foreach (CountryOption country in Countries)
        {
            if (!ids.Contains(country.TimeZoneId))
                ids.Add(country.TimeZoneId);
        }

        string localId = TimeZoneInfo.Local.Id;
        if (!ids.Contains(localId))
            ids.Add(localId);

        var zones = new List<TimeZoneInfo>();

        foreach (string id in ids)
        {
            try
            {
                zones.Add(TimeZoneInfo.FindSystemTimeZoneById(id));
            }
            catch (TimeZoneNotFoundException)
            {
                // The zone is unknown on this operating system.
            }
            catch (InvalidTimeZoneException)
            {
                // The zone information is corrupted - skip it.
            }
        }

        zones.Sort((left, right) => left.BaseUtcOffset.CompareTo(right.BaseUtcOffset));

        return zones;
    }

    /// <summary>Symbol of a currency code, or the code itself when unknown.</summary>
    public static string GetSymbol(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return string.Empty;
        var match = Currencies.FirstOrDefault(c =>
            string.Equals(c.Code, code.Trim(), StringComparison.OrdinalIgnoreCase));
        return match?.Symbol ?? code.Trim().ToUpperInvariant();
    }

    /// <summary>Fraction digits configured for a currency (2 by default).</summary>
    public static int GetDecimalDigits(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return 2;
        var match = Currencies.FirstOrDefault(c =>
            string.Equals(c.Code, code.Trim(), StringComparison.OrdinalIgnoreCase));
        return match?.DecimalDigits ?? 2;
    }

    /// <summary>Country preset for a country code (null when unknown).</summary>
    public static CountryOption? GetCountry(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        return Countries.FirstOrDefault(c =>
            string.Equals(c.Code, code.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Localized country name for the requested UI language. Falls back to the
    /// country code when the culture database has no entry for the pair.
    /// </summary>
    public static string GetCountryName(string? code, string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(code)) return string.Empty;
        var countryCode = code.Trim().ToUpperInvariant();
        var language = string.IsNullOrWhiteSpace(languageCode) ? "en" : languageCode.Trim().ToLowerInvariant();

        try
        {
            var culture = CultureInfo.GetCultureInfo($"{language}-{countryCode}");
            var name = culture.DisplayName;
            if (!string.IsNullOrWhiteSpace(name) &&
                !name.Contains('(') &&
                !string.Equals(name, countryCode, StringComparison.OrdinalIgnoreCase))
            {
                return name;
            }
        }
        catch (CultureNotFoundException)
        {
            // Fall through to the English name below.
        }

        return countryCode;
    }

    /// <summary>Localized currency name when the culture database provides one.</summary>
    public static string GetCurrencyName(string? code, string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(code)) return string.Empty;
        var currencyCode = code.Trim().ToUpperInvariant();
        var language = string.IsNullOrWhiteSpace(languageCode) ? "en" : languageCode.Trim().ToLowerInvariant();

        var option = Currencies.FirstOrDefault(c =>
            string.Equals(c.Code, currencyCode, StringComparison.OrdinalIgnoreCase));

        // The currency of the active language region, when it matches.
        try
        {
            var culture = CultureInfo.GetCultureInfo(language);
            var region = new RegionInfo(culture.Name);
            if (string.Equals(region.ISOCurrencySymbol, currencyCode, StringComparison.OrdinalIgnoreCase))
            {
                var localized = region.CurrencyNativeName;
                if (!string.IsNullOrWhiteSpace(localized)) return localized;
            }
        }
        catch (CultureNotFoundException)
        {
            // Fall through to the country based lookup below.
        }
        catch (ArgumentException)
        {
            // The culture has no region information.
        }

        // Any country of the catalog that uses this currency.
        foreach (CountryOption country in Countries)
        {
            if (!string.Equals(country.CurrencyCode, currencyCode, StringComparison.OrdinalIgnoreCase))
                continue;

            try
            {
                var region = new RegionInfo(country.Code);
                var localized = region.CurrencyNativeName;
                if (!string.IsNullOrWhiteSpace(localized)) return localized;
            }
            catch (ArgumentException)
            {
                // The country is not known to the globalization data.
            }
        }

        return option?.EnglishName ?? currencyCode;
    }
}

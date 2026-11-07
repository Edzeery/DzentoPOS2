using System.Globalization;

namespace DzentoPOS2.Shared.Helpers;

/// <summary>
/// Runtime holder of the store's regional configuration (currency, country and
/// time zone). <para>
/// Timestamps are always persisted in UTC. The configured time zone is only used
/// to derive the "local" day/month boundaries used by reports and to render
/// dates for the user - it never changes what is written to the database.
/// </para>
/// </summary>
public sealed class RegionalContext
{
    private string _timeZoneId = TimeZoneInfo.Local.Id;
    private TimeZoneInfo _timeZone = TimeZoneInfo.Local;

    /// <summary>ISO-4217 code of the currency used by the store.</summary>
    public string CurrencyCode { get; set; } = "DZD";

    /// <summary>ISO-3166-1 alpha-2 code of the country the store operates in.</summary>
    public string CountryCode { get; set; } = "DZ";

    /// <summary>Symbol of <see cref="CurrencyCode"/>.</summary>
    public string CurrencySymbol => RegionalCatalog.GetSymbol(CurrencyCode);

    /// <summary>Fraction digits of <see cref="CurrencyCode"/>.</summary>
    public int CurrencyDecimalDigits => RegionalCatalog.GetDecimalDigits(CurrencyCode);

    /// <summary>
    /// Optional override for the thousands separator. When null the explicit
    /// language policy from <see cref="CreateNumberCulture"/> is used.
    /// </summary>
    public string? NumberGroupSeparator { get; set; }

    /// <summary>
    /// Optional override for the decimal separator. When null the explicit
    /// language policy from <see cref="CreateNumberCulture"/> is used.
    /// </summary>
    public string? NumberDecimalSeparator { get; set; }

    /// <summary>NO-BREAK SPACE used as the thousands separator for ar/fr.</summary>
    public const string NoBreakSpace = "\u00A0";

    /// <summary>IANA identifier of the configured time zone.</summary>
    public string TimeZoneId
    {
        get => _timeZoneId;
        set
        {
            var resolved = ResolveTimeZone(value);
            _timeZone = resolved;
            _timeZoneId = resolved.Id;
        }
    }

    /// <summary>The resolved time zone (never null).</summary>
    public TimeZoneInfo TimeZone => _timeZone;

    /// <summary>Current UTC timestamp (storage format).</summary>
    public DateTime UtcNow => DateTime.UtcNow;

    /// <summary>Current timestamp in the configured time zone (display format).</summary>
    public DateTime Now => ToLocal(DateTime.UtcNow);

    /// <summary>Midnight of the current day in the configured time zone.</summary>
    public DateTime Today => Now.Date;

    /// <summary>
    /// Converts a stored timestamp to the configured time zone. Values without
    /// an explicit kind are treated as UTC because that is how the application
    /// stores every timestamp.
    /// </summary>
    public DateTime ToLocal(DateTime value)
    {
        var utc = value.Kind == DateTimeKind.Utc
            ? value
            : value.Kind == DateTimeKind.Local
                ? value.ToUniversalTime()
                : DateTime.SpecifyKind(value, DateTimeKind.Utc);

        return TimeZoneInfo.ConvertTimeFromUtc(utc, _timeZone);
    }

    /// <summary>
    /// Converts a local (store time) timestamp back to UTC so it can be used in
    /// database comparisons.
    /// </summary>
    public DateTime ToUtc(DateTime local)
    {
        var unspecified = DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(unspecified, _timeZone);
    }

    /// <summary>
    /// Same as <see cref="ToUtc"/> but never throws: local times that do not
    /// exist (daylight saving gaps) are treated as UTC instead of breaking the
    /// report that requested them.
    /// </summary>
    public DateTime ToUtcSafe(DateTime local)
    {
        try
        {
            return ToUtc(local);
        }
        catch (ArgumentException)
        {
            return DateTime.SpecifyKind(local, DateTimeKind.Utc);
        }
    }

    /// <summary>
    /// Start of the given local day expressed in UTC - the value that can be
    /// compared directly against UTC stored columns.
    /// </summary>
    public DateTime StartOfLocalDayUtc(DateTime localDay) => ToUtc(localDay.Date);

    /// <summary>Start of the current local month expressed in UTC.</summary>
    public DateTime StartOfLocalMonthUtc()
    {
        var local = Now;
        return ToUtc(new DateTime(local.Year, local.Month, 1));
    }

    /// <summary>Formats an amount using the configured currency.</summary>
    public string FormatMoney(decimal amount, string? currencyCode = null)
    {
        var code = string.IsNullOrWhiteSpace(currencyCode) ? CurrencyCode : currencyCode;
        var digits = RegionalCatalog.GetDecimalDigits(code);
        return amount.ToString("N" + digits, NumberCultureFor(CultureInfo.CurrentCulture)) + " " +
               RegionalCatalog.GetSymbol(code);
    }

    /// <summary>
    /// Number culture for this store, applying the configured separator
    /// overrides on top of the explicit language policy.
    /// </summary>
    public CultureInfo NumberCultureFor(CultureInfo culture)
        => CreateNumberCulture(culture, NumberGroupSeparator, NumberDecimalSeparator);

    /// <summary>
    /// Builds the one and only number culture used for display. It is explicit
    /// (never user overrides), always Western digits, and guarantees that the
    /// thousands separator differs from the decimal separator.
    ///
    /// Policy: <c>en</c> uses "," / "."; <c>ar</c> and <c>fr</c> use a
    /// NO-BREAK SPACE (U+00A0) for thousands and "," for decimals. Other
    /// languages keep their own separators, normalized and forced distinct.
    /// </summary>
    public static CultureInfo CreateNumberCulture(
        CultureInfo? culture = null,
        string? groupSeparator = null,
        string? decimalSeparator = null)
    {
        culture ??= CultureInfo.CurrentCulture;

        var name = string.IsNullOrWhiteSpace(culture.Name) ? "en-US" : culture.Name;
        var result = new CultureInfo(name, useUserOverride: false);
        var number = (NumberFormatInfo)result.NumberFormat.Clone();

        // Western digits everywhere (rule 3).
        number.NativeDigits = new[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        number.DigitSubstitution = DigitShapes.None;
        number.NegativeSign = "-";
        number.PositiveSign = "+";

        var group = groupSeparator ?? DefaultGroupSeparator(result);
        var dec = decimalSeparator ?? DefaultDecimalSeparator(result);

        if (string.IsNullOrEmpty(group)) group = ",";
        if (string.IsNullOrEmpty(dec)) dec = ".";

        if (string.Equals(group, dec, StringComparison.Ordinal))
            dec = group == "," ? "." : ",";

        number.NumberGroupSeparator = group;
        number.NumberDecimalSeparator = dec;
        number.CurrencyGroupSeparator = group;
        number.CurrencyDecimalSeparator = dec;

        result.NumberFormat = number;
        return result;
    }

    /// <summary>Default thousands separator for the UI language.</summary>
    public static string DefaultGroupSeparator(CultureInfo culture)
    {
        culture ??= CultureInfo.CurrentCulture;

        switch (culture.TwoLetterISOLanguageName)
        {
            case "ar":
            case "fr":
                return NoBreakSpace;

            case "en":
                return ",";

            default:
                var raw = culture.NumberFormat.NumberGroupSeparator;
                return raw == "\u066C" ? "," : raw;
        }
    }

    /// <summary>Default decimal separator for the UI language.</summary>
    public static string DefaultDecimalSeparator(CultureInfo culture)
    {
        culture ??= CultureInfo.CurrentCulture;

        switch (culture.TwoLetterISOLanguageName)
        {
            case "ar":
            case "fr":
                return ",";

            case "en":
                return ".";

            default:
                var raw = culture.NumberFormat.NumberDecimalSeparator;
                return raw == "\u066B" ? "." : raw;
        }
    }

    /// <summary>Resolves a time zone by id, falling back to the local one.</summary>
    public static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
            return TimeZoneInfo.Local;

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId.Trim());
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.Local;
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.Local;
        }
    }
}

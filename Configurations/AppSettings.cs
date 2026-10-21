namespace DzentoPOS2.WinForms.Configurations;

/// <summary>
/// User preferences persisted next to the application (user-settings.json).
/// Every value here is a per-store/per-user preference; the light/dark theme
/// itself is owned by <see cref="Themes.ThemeManager"/>.
/// </summary>
public class AppSettings
{
    public string Language { get; set; } = "ar";

    public bool RememberMe { get; set; }

    public string LastUserName { get; set; } = "";

    public bool DarkMode { get; set; }

    // ---------------- Store branding ----------------

    /// <summary>Display name of the store, shown in the shell and on receipts.</summary>
    public string StoreName { get; set; } = "";

    /// <summary>Absolute path of the custom store logo (empty = built-in logo).</summary>
    public string StoreLogoPath { get; set; } = "";

    // ---------------- Regional ----------------

    /// <summary>ISO-4217 code of the currency used by the store.</summary>
    public string CurrencyCode { get; set; } = "SAR";

    /// <summary>ISO-3166-1 alpha-2 code of the country the store operates in.</summary>
    public string CountryCode { get; set; } = "SA";

    /// <summary>IANA id of the time zone used for reports and date display.</summary>
    public string TimeZoneId { get; set; } = "";

    // ---------------- Printing ----------------

    /// <summary>Name of the printer used for receipts and reports.</summary>
    public string PrinterName { get; set; } = "";

    /// <summary>Paper size name (for example A4, Letter, Roll80).</summary>
    public string PaperSize { get; set; } = "A4";

    /// <summary>True when the printer output should be landscape.</summary>
    public bool Landscape { get; set; }

    /// <summary>Receipt width in millimetres (58 or 80 for thermal printers).</summary>
    public int ReceiptWidthMm { get; set; } = 80;

    // ---------------- Barcode scanning ----------------

    /// <summary>Enables the camera barcode scanner inside the sales screen.</summary>
    public bool CameraBarcodeEnabled { get; set; } = true;

    /// <summary>Name of the DirectShow video device used for scanning.</summary>
    public string BarcodeCameraDevice { get; set; } = "";
}

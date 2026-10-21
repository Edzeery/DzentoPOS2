using DzentoPOS2.Shared.Helpers;
using DzentoPOS2.WinForms.Helpers;

namespace DzentoPOS2.WinForms.Configurations;

public sealed class CurrentSettingsService : ICurrentSettingsService
{
    private readonly IUserPreferencesService _preferences;
    private AppSettings _current;

    public AppSettings Current => _current;

    public RegionalContext Regional { get; }

    public event EventHandler? Changed;

    public CurrentSettingsService(
        IUserPreferencesService preferences,
        RegionalContext regional)
    {
        _preferences = preferences;
        Regional = regional;
        _current = _preferences.Load();
        Apply(_current);
    }

    public void Save(AppSettings settings)
    {
        _current = settings ?? new AppSettings();
        Apply(_current);

        try
        {
            _preferences.Save(_current);
        }
        catch (Exception ex)
        {
            AppMessageBox.Error(ex.Message);
            return;
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Update(Action<AppSettings> mutate)
    {
        var copy = Clone(_current);
        mutate(copy);
        Save(copy);
    }

    private void Apply(AppSettings settings)
    {
        Regional.CurrencyCode = string.IsNullOrWhiteSpace(settings.CurrencyCode)
            ? "SAR"
            : settings.CurrencyCode.Trim().ToUpperInvariant();

        Regional.CountryCode = string.IsNullOrWhiteSpace(settings.CountryCode)
            ? "SA"
            : settings.CountryCode.Trim().ToUpperInvariant();

        if (!string.IsNullOrWhiteSpace(settings.TimeZoneId))
            Regional.TimeZoneId = settings.TimeZoneId;
    }

    internal static AppSettings Clone(AppSettings source) => new()
    {
        Language = source.Language,
        RememberMe = source.RememberMe,
        LastUserName = source.LastUserName,
        DarkMode = source.DarkMode,
        StoreName = source.StoreName,
        StoreLogoPath = source.StoreLogoPath,
        CurrencyCode = source.CurrencyCode,
        CountryCode = source.CountryCode,
        TimeZoneId = source.TimeZoneId,
        PrinterName = source.PrinterName,
        PaperSize = source.PaperSize,
        Landscape = source.Landscape,
        ReceiptWidthMm = source.ReceiptWidthMm,
        CameraBarcodeEnabled = source.CameraBarcodeEnabled,
        BarcodeCameraDevice = source.BarcodeCameraDevice
    };
}

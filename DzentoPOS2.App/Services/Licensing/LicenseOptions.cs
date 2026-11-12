using Microsoft.Extensions.Configuration;

namespace DzentoPOS2.App.Services.Licensing;

/// <summary>
/// Runtime configuration for the offline licence system.
///
/// <para>
/// The public key defaults to <see cref="LicensePublicKey.Pem"/> and is
/// deliberately <b>not</b> read from configuration — a customer must not be able
/// to swap it for their own key and sign licences. Only the tuning values
/// (enforcement switch, grace windows, trial plan) come from the
/// <c>Licensing</c> configuration section. Tests inject their own
/// <see cref="LicenseOptions"/> instance directly.
/// </para>
/// </summary>
public sealed class LicenseOptions
{
    /// <summary>
    /// Vendor RSA public key (PEM). Defaults to the embedded
    /// <see cref="LicensePublicKey.Pem"/>; overridable only in code/tests.
    /// </summary>
    public string? PublicKeyPem { get; set; } = LicensePublicKey.Pem;

    /// <summary>
    /// When <c>false</c> (the default) every guard is a no-op, so development
    /// and the automated tests run unconstrained. Production sets it to
    /// <c>true</c> in <c>appsettings.json</c>.
    /// </summary>
    public bool Enforce { get; set; }

    /// <summary>Days after expiry during which the app stays writable.</summary>
    public int GraceDays { get; set; } = 7;

    /// <summary>Days before expiry at which the UI starts warning.</summary>
    public int ExpiringSoonDays { get; set; } = 14;

    /// <summary>Plan whose limits apply while the app is unlicensed.</summary>
    public string DefaultPlanName { get; set; } = "Basic";

    public static LicenseOptions FromConfiguration(IConfiguration? configuration)
    {
        var options = new LicenseOptions();
        var section = configuration?.GetSection("Licensing");
        if (section is null)
            return options;

        if (bool.TryParse(section["Enforce"], out var enforce))
            options.Enforce = enforce;

        if (int.TryParse(section["GraceDays"], out var graceDays))
            options.GraceDays = graceDays;

        if (int.TryParse(section["ExpiringSoonDays"], out var expiringSoonDays))
            options.ExpiringSoonDays = expiringSoonDays;

        if (!string.IsNullOrWhiteSpace(section["DefaultPlanName"]))
            options.DefaultPlanName = section["DefaultPlanName"]!;

        return options;
    }
}

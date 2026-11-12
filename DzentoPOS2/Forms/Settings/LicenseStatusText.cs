using System.Globalization;
using DzentoPOS2.App.Services.Licensing;
using DzentoPOS2.Localization.Resources;

namespace DzentoPOS2.WinForms.Forms.Settings;

/// <summary>Shared formatting of a <see cref="LicenseStatus"/> for the UI.</summary>
internal static class LicenseStatusText
{
    public static string State(LicenseState state) => state switch
    {
        LicenseState.Active => AppStrings.LicenseStateActive,
        LicenseState.ExpiringSoon => AppStrings.LicenseStateExpiringSoon,
        LicenseState.GracePeriod => AppStrings.LicenseStateGrace,
        LicenseState.Expired => AppStrings.LicenseStateExpired,
        LicenseState.DeviceMismatch => AppStrings.LicenseStateDeviceMismatch,
        _ => AppStrings.LicenseStateUnlicensed
    };

    public static string Limit(int value) =>
        value == int.MaxValue
            ? AppStrings.LicenseUnlimited
            : value.ToString(CultureInfo.InvariantCulture);

    public static string Summary(LicenseStatus status)
    {
        var expiry = status.ExpiryDate is { } date
            ? $"{AppStrings.LicenseExpiryLabel}: {date:yyyy-MM-dd}"
            : $"{AppStrings.LicenseExpiryLabel}: {AppStrings.NoExpiry}";

        var limits = string.Format(
            AppStrings.LicenseLimitsFormat,
            Limit(status.MaxUsers),
            Limit(status.MaxBranches));

        return $"{State(status.State)}   |   " +
               $"{AppStrings.LicensePlanLabel}: {status.PlanName ?? "-"}   |   " +
               $"{expiry}{Environment.NewLine}{limits}";
    }
}
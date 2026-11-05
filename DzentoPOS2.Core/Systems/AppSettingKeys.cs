namespace DzentoPOS2.Core.Systems;

/// <summary>
/// Stable keys of the application settings stored in the <c>AppSettings</c>
/// table. Shared by the seeder and the services that read them.
/// </summary>
public static class AppSettingKeys
{
    /// <summary>Maximum invoice discount (percent) a cashier may apply.</summary>
    public const string MaxDiscountPercent = "sales.max_discount_percent";

    /// <summary>Activity modules (feature flags), group "Modules".</summary>
    public const string ModuleGrocery = "modules.grocery.enabled";
    public const string ModulePhones = "modules.phones.enabled";
    public const string ModuleDigital = "modules.digital.enabled";
    public const string ModuleGeneral = "modules.general.enabled";
    public const string ModulesConfigured = "modules.configured";

    /// <summary>Catalog: allow duplicate barcodes (shared barcode picker in POS).</summary>
    public const string AllowDuplicateBarcodes = "catalog.allow_duplicate_barcodes";
}
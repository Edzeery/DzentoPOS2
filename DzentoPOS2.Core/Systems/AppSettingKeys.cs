namespace DzentoPOS2.Core.Systems;

/// <summary>
/// Stable keys of the application settings stored in the <c>AppSettings</c>
/// table. Shared by the seeder and the services that read them.
/// </summary>
public static class AppSettingKeys
{
    /// <summary>Maximum invoice discount (percent) a cashier may apply.</summary>
    public const string MaxDiscountPercent = "sales.max_discount_percent";
}
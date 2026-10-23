using DzentoPOS2.App;
using DzentoPOS2.Data;
using DzentoPOS2.Localization;
using DzentoPOS2.WinForms.Configurations;
using DzentoPOS2.WinForms.Forms.Accounting;
using DzentoPOS2.WinForms.Forms.Auth;
using DzentoPOS2.WinForms.Forms.Customers;
using DzentoPOS2.WinForms.Forms.Dashboard;
using DzentoPOS2.WinForms.Forms.Identity;
using DzentoPOS2.WinForms.Forms.Inventory;
using DzentoPOS2.WinForms.Forms.Layout;
using DzentoPOS2.WinForms.Forms.Purchases;
using DzentoPOS2.WinForms.Forms.Reports;
using DzentoPOS2.WinForms.Forms.Sales;
using DzentoPOS2.WinForms.Forms.Settings;
using DzentoPOS2.WinForms.Forms.Suppliers;
using DzentoPOS2.WinForms.Navigation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DzentoPOS2.WinForms.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddWinForms(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Localization
        services.AddLocalizationModule();

        // Logging (required by AutoMapper 16.x)
        services.AddLogging();

        // App Layer
        services.AddApplicationServices();

        // Data Layer
        services.AddDataServices(configuration);

        // Application Context
        services.AddSingleton<AppApplicationContext>();
        services.AddSingleton<IUserPreferencesService, UserPreferencesService>();
        services.AddSingleton<ICurrentSettingsService, CurrentSettingsService>();
        services.AddSingleton<INavigationService, NavigationService>();
       
        // Forms
        services.AddTransient<LoginForm>();
        services.AddTransient<MainForm>();
        services.AddTransient<DashboardForm>();
        services.AddTransient<UsersForm>();
        services.AddTransient<RolesForm>();

        services.AddTransient<ProductsForm>();
        services.AddTransient<PurchasesForm>();
        services.AddTransient<SalesForm>();
        services.AddTransient<CustomersForm>();
        services.AddTransient<SuppliersForm>();
        services.AddTransient<InventoryForm>();
        services.AddTransient<ReportsForm>();
        services.AddTransient<SettingsForm>();
        services.AddTransient<ExpensesForm>();
        services.AddTransient<ProductCategoriesForm>();
        services.AddTransient<ProductUnitsForm>();

        return services;
    }
}
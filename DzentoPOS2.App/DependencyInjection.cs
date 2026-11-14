using DzentoPOS2.App.Authentication;
using DzentoPOS2.App.Authorization;
using DzentoPOS2.App.Services.Accounting;
using DzentoPOS2.App.Services.Businesses;
using DzentoPOS2.App.Services.Events;
using DzentoPOS2.App.Services.Identity;
using DzentoPOS2.App.Services.Identity.Interfaces;
using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.App.Services.Licensing;
using DzentoPOS2.App.Services.Partners;
using DzentoPOS2.App.Services.Purchases;
using DzentoPOS2.App.Services.Reports;
using DzentoPOS2.App.Services.Reports.Interfaces;
using DzentoPOS2.App.Services.Sales;
using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.App.Session;
using DzentoPOS2.Core.Services;
using DzentoPOS2.Data.Context;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace DzentoPOS2.App
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services)
        {
            services.AddAutoMapper(cfg => { },
                Assembly.GetExecutingAssembly());

            services.AddValidatorsFromAssembly(
                Assembly.GetExecutingAssembly());

            // Regional configuration (currency / country / time zone) is shared
            // by the application layer and the user interface.
            services.AddSingleton<Shared.Helpers.RegionalContext>();
            services.AddSingleton<IBusinessClock, BusinessClock>();
            services.AddScoped<IStoreContextService, StoreContextService>();
            services.AddSingleton<IBranchResolver, BranchResolver>();
            services.AddSingleton<IActiveBranchContext, ActiveBranchContext>();
            services.AddSingleton<IBranchSwitchGuardRegistry, BranchSwitchGuardRegistry>();
            services.AddTransient<IBranchSwitchGuard, BranchSwitchGuard>();
            services.AddScoped<ICapabilityService, CapabilityService>();
            services.AddScoped<IBranchLimitPolicy, LicenseBranchLimitPolicy>();

            // Offline licensing: tuning comes from the "Licensing" configuration
            // section (the public key is embedded in the assembly). The device
            // identifier is replaced by a Windows-specific implementation in the
            // WinForms layer when available. TryAdd lets tests register their own
            // LicenseOptions (with a test key) before this call.
            services.TryAddSingleton(sp =>
                LicenseOptions.FromConfiguration(sp.GetService<IConfiguration>()));
            services.TryAddSingleton<IDeviceIdentifier, DefaultDeviceIdentifier>();
            services.AddScoped<ILicenseService, LicenseService>();

            services.AddScoped<IDocumentNumberService, DocumentNumberService>();
            services.AddScoped<IBackupService>(sp =>
    new BackupService(
        sp.GetRequiredService<IDbContextFactory<AppDbContext>>(),
        backupDirectory: null,
        guard: sp.GetService<IAuthorizationGuard>()));

            services.AddScoped<IAuthService, AuthService>();

            services.AddSingleton<CurrentUserService>();
            services.AddSingleton<ICurrentUserService>(sp => sp.GetRequiredService<CurrentUserService>());
            services.AddSingleton<ICurrentUserAccessor>(sp => sp.GetRequiredService<CurrentUserService>());

            // Singleton so every scoped service and every form of the running
            // shell share the same sale / cash-drawer notifications.
            services.AddSingleton<ISalesEventPublisher, SalesEventPublisher>();

            services.AddSingleton<IInventoryChangedNotifier, InventoryChangedNotifier>();
            services.AddScoped<IInventoryValuationService, InventoryValuationService>();
            services.AddScoped<IInventorySnapshotService, InventorySnapshotService>();

            services.AddScoped<IPermissionService,
                PermissionService>();

            services.AddScoped<IAuthorizationGuard,
                AuthorizationGuard>();

            services.AddScoped<IAuditService,
                AuditService>();

    services.AddScoped<IPasswordHasher,
        PasswordHasher>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<
                    IPermissionManagementService,
                    PermissionManagementService>();

            services.AddScoped<
                IDashboardService,
                DashboardService>();

            services.AddScoped<
                ICrossBranchReportService,
                CrossBranchReportService>();

            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IBarcodeService, BarcodeService>();
            services.AddScoped<ISerialUnitService, SerialUnitService>();
            services.AddScoped<IDigitalCodeService, DigitalCodeService>();
            services.AddScoped<IBatchService, BatchService>();
            services.AddScoped<IPosProductStore, PosProductStore>();
            services.AddScoped<IPosBarcodeResolver, PosBarcodeResolver>();
            services.AddSingleton<ICatalogChangeNotifier, CatalogChangeNotifier>();
            services.AddScoped<IStockService, StockService>();
            services.AddScoped<IExpenseService, ExpenseService>();

            services.AddScoped<IProductCategoryService, ProductCategoryService>();
            services.AddScoped<IProductUnitService, ProductUnitService>();

            services.AddScoped<ISaleService, SaleService>();
            services.AddScoped<ISalesPolicy, SalesPolicy>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<ISupplierService, SupplierService>();
            services.AddScoped<IPurchaseService, PurchaseService>();

            services.AddScoped<ICashSessionService, CashSessionService>();

            services.AddScoped<IBranchService, BranchService>();

            services.AddScoped<IBranchResourceService, BranchResourceService>();

            services.AddScoped<IModuleService, ModuleService>();

            return services;
        }
    }
}
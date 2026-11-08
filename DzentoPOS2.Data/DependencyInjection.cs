using DzentoPOS2.Core.Configuration;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Accounting;
using DzentoPOS2.Data.Repositories.Accounting.Interfaces;
using DzentoPOS2.Data.Repositories.Businesses;
using DzentoPOS2.Data.Repositories.Businesses.Interfaces;
using DzentoPOS2.Data.Repositories.Common;
using DzentoPOS2.Data.Repositories.Common.Interfaces;
using DzentoPOS2.Data.Repositories.Identity;
using DzentoPOS2.Data.Repositories.Identity.Interfaces;
using DzentoPOS2.Data.Repositories.Inventory;
using DzentoPOS2.Data.Repositories.Inventory.Interfaces;
using DzentoPOS2.Data.Repositories.Licensing;
using DzentoPOS2.Data.Repositories.Licensing.Interfaces;
using DzentoPOS2.Data.Repositories.Partners;
using DzentoPOS2.Data.Repositories.Partners.Interfaces;
using DzentoPOS2.Data.Repositories.Purchases;
using DzentoPOS2.Data.Repositories.Purchases.Interfaces;
using DzentoPOS2.Data.Repositories.Reports;
using DzentoPOS2.Data.Repositories.Reports.Interfaces;
using DzentoPOS2.Data.Repositories.Sales;
using DzentoPOS2.Data.Repositories.Sales.Interfaces;
using DzentoPOS2.Data.Repositories.Systems;
using DzentoPOS2.Data.Repositories.Systems.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using DzentoPOS2.Data.Interceptors;

namespace DzentoPOS2.Data
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDataServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {

            services.AddDbContext<AppDbContext>(options =>
            {
                var connectionString =
                    configuration.GetConnectionString("DefaultConnection");

                options.UseSqlite(
                    ResolveSqliteConnectionString(connectionString));

                options.AddInterceptors(
                    SqliteConnectionInterceptor.Instance,
                    AuditSaveChangesInterceptor.Instance);

#if DEBUG
                options.EnableSensitiveDataLogging();

                options.EnableDetailedErrors();
#endif
            }, ServiceLifetime.Scoped);

            services.AddDbContextFactory<AppDbContext>(options =>
            {
                var connectionString =
                    configuration.GetConnectionString("DefaultConnection");

                options.UseSqlite(
                    ResolveSqliteConnectionString(connectionString));

                options.AddInterceptors(
                    SqliteConnectionInterceptor.Instance,
                    AuditSaveChangesInterceptor.Instance);

#if DEBUG
                options.EnableSensitiveDataLogging();

                options.EnableDetailedErrors();
#endif
            }, ServiceLifetime.Scoped);

            #region Common Repositories

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            #endregion

            #region Identity Repositories

            services.AddScoped<IUserRepository, UserRepository>(); 
            services.AddScoped<IRoleRepository, RoleRepository>(); 
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddScoped<IUserRoleRepository, UserRoleRepository>(); 
            services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();

            #endregion

            #region Inventory Repositories

            services.AddScoped<IProductRepository, ProductRepository>();

            services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();

            services.AddScoped<IProductBarcodeRepository, ProductBarcodeRepository>();

            services.AddScoped<IProductPriceRepository, ProductPriceRepository>();

            services.AddScoped<IProductStockRepository, ProductStockRepository>();

            services.AddScoped<IProductUnitRepository, ProductUnitRepository>();

            services.AddScoped<IProductVariantRepository, ProductVariantRepository>();

            services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepository>();

            services.AddScoped<IStockAdjustmentRepository, StockAdjustmentRepository>();

            services.AddScoped<IStockTransferRepository, StockTransferRepository>();

            services.AddScoped<IStockTransferItemRepository, StockTransferItemRepository>();

            #endregion

            #region Businesses Repositories

            services.AddScoped<IBranchRepository, BranchRepository>();

            services.AddScoped<ICashRegisterRepository, CashRegisterRepository>();

            services.AddScoped<IWarehouseRepository, WarehouseRepository>();

            #endregion

            #region Partners Repositories

            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<ICustomerGroupRepository, CustomerGroupRepository>();
            services.AddScoped<ISupplierRepository, SupplierRepository>();
            services.AddScoped<ISupplierPaymentRepository, SupplierPaymentRepository>();

            #endregion

            #region Sales Repositories

            services.AddScoped<ISalesInvoiceRepository, SalesInvoiceRepository>();
            services.AddScoped<ISalesInvoiceItemRepository, SalesInvoiceItemRepository>();
            services.AddScoped<ISalesPaymentRepository, SalesPaymentRepository>();
            services.AddScoped<ISalesQuotationRepository, SalesQuotationRepository>();
            services.AddScoped<ISalesReturnRepository, SalesReturnRepository>();
            services.AddScoped<ISalesReturnItemRepository, SalesReturnItemRepository>();

            #endregion

            #region Purchases Repositories

            services.AddScoped<IPurchaseInvoiceRepository, PurchaseInvoiceRepository>();
            services.AddScoped<IPurchaseInvoiceItemRepository, PurchaseInvoiceItemRepository>();
            services.AddScoped<IPurchasePaymentRepository, PurchasePaymentRepository>();
            services.AddScoped<IPurchaseReturnRepository, PurchaseReturnRepository>();
            services.AddScoped<IPurchaseReturnItemRepository, PurchaseReturnItemRepository>();

            #endregion

            #region Accounting Repositories

            services.AddScoped<ICashboxRepository, CashboxRepository>();

            services.AddScoped<ICashTransactionRepository, CashTransactionRepository>();

            services.AddScoped<ICashSessionRepository, CashSessionRepository>();

            services.AddScoped<ICurrencyRepository, CurrencyRepository>();

            services.AddScoped<IExpenseRepository, ExpenseRepository>();

            services.AddScoped<IExpenseCategoryRepository, ExpenseCategoryRepository>();

            services.AddScoped<IFinancialPeriodRepository, FinancialPeriodRepository>();

            services.AddScoped<ITaxRepository, TaxRepository>();

            #endregion

            #region Licensing Repositories

            services.AddScoped<ILicenseRepository, LicenseRepository>();

            services.AddScoped<ILicensePlanRepository, LicensePlanRepository>();

            services.AddScoped<IDeviceActivationRepository, DeviceActivationRepository>();

            #endregion

            #region Systems Repositories

            services.AddScoped<IActivityLogRepository, ActivityLogRepository>();

            services.AddScoped<IAppSettingRepository, AppSettingRepository>();

            services.AddScoped<IAttachmentRepository, AttachmentRepository>();

            services.AddScoped<IAuditLogRepository, AuditLogRepository>();

            services.AddScoped<IErrorLogRepository, ErrorLogRepository>();

            services.AddScoped<INotificationRepository, NotificationRepository>();

            services.AddScoped<ISequenceRepository, SequenceRepository>();

            #endregion

            #region Reports Repositories

            services.AddScoped<IDashboardWidgetRepository, DashboardWidgetRepository>();

            services.AddScoped<IReportTemplateRepository, ReportTemplateRepository>();

            #endregion

            return services;
        }

        /// <summary>
        /// Makes a relative SQLite <c>Data Source</c> absolute against the
        /// per-user data folder (<c>%LocalAppData%\DzentoPOS2\Database</c>) so
        /// the database is never written inside the installation directory
        /// (which may be read-only, and must survive an upgrade).
        /// A database left next to the executable by an older version is copied
        /// there once, verified, and the original is kept.
        /// </summary>
        private static string ResolveSqliteConnectionString(string? connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return connectionString ?? string.Empty;

            const string key = "Data Source=";
            int keyIndex = connectionString.IndexOf(key, StringComparison.OrdinalIgnoreCase);
            if (keyIndex < 0)
                return connectionString;

            int valueStart = keyIndex + key.Length;
            int valueEnd = connectionString.IndexOf(';', valueStart);

            string value = (valueEnd < 0
                ? connectionString.Substring(valueStart)
                : connectionString.Substring(valueStart, valueEnd - valueStart)).Trim();

            if (string.IsNullOrWhiteSpace(value) || Path.IsPathRooted(value))
                return connectionString;

            AppPaths.EnsureCreated();

            string legacy = Path.GetFullPath(
                Path.Combine(AppContext.BaseDirectory, value));

            string absolute = Path.Combine(
                AppPaths.DatabaseFolder,
                Path.GetFileName(value));

            AppPaths.MigrateLegacyDatabase(legacy, absolute);

            return valueEnd < 0
                ? connectionString.Substring(0, valueStart) + absolute
                : connectionString.Substring(0, valueStart) + absolute + connectionString.Substring(valueEnd);
        }
    }
}
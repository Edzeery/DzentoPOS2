using DzentoPOS2.Core.Common;
using DzentoPOS2.Core.Entities.Accounting;
using DzentoPOS2.Core.Entities.Businesses;
using DzentoPOS2.Core.Entities.Identity;
using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Core.Entities.Licensing;
using DzentoPOS2.Core.Entities.Partners;
using DzentoPOS2.Core.Entities.Purchases;
using DzentoPOS2.Core.Entities.Reports;
using DzentoPOS2.Core.Entities.Sales;
using DzentoPOS2.Core.Entities.Systems;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
         
        #region Identity

         
        public DbSet<User> Users => Set<User>();

        public DbSet<Role> Roles => Set<Role>();

        public DbSet<Permission> Permissions => Set<Permission>();

        public DbSet<UserRole> UserRoles => Set<UserRole>();

        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

        #endregion

        #region Businesses

        public DbSet<Branch> Branches => Set<Branch>();

        public DbSet<Warehouse> Warehouses => Set<Warehouse>();

        public DbSet<CashRegister> CashRegisters => Set<CashRegister>();

        #endregion

        #region Inventory

        public DbSet<Product> Products => Set<Product>();

        public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();

        public DbSet<ProductUnit> ProductUnits => Set<ProductUnit>();

        public DbSet<ProductBarcode> ProductBarcodes => Set<ProductBarcode>();

        public DbSet<ProductPrice> ProductPrices => Set<ProductPrice>();

        public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();

        public DbSet<ProductStock> ProductStocks => Set<ProductStock>();

        public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();

        public DbSet<StockAdjustment> StockAdjustments => Set<StockAdjustment>();

        public DbSet<StockTransfer> StockTransfers => Set<StockTransfer>();

        public DbSet<StockTransferItem> StockTransferItems => Set<StockTransferItem>();

        #endregion

        #region Partners

        public DbSet<Customer> Customers => Set<Customer>();

        public DbSet<CustomerGroup> CustomerGroups => Set<CustomerGroup>();

        public DbSet<Supplier> Suppliers => Set<Supplier>();

        public DbSet<SupplierPayment> SupplierPayments => Set<SupplierPayment>();

        #endregion

        #region Sales

        public DbSet<SalesInvoice> SalesInvoices => Set<SalesInvoice>();

        public DbSet<SalesInvoiceItem> SalesInvoiceItems => Set<SalesInvoiceItem>();

        public DbSet<SalesReturn> SalesReturns => Set<SalesReturn>();

        public DbSet<SalesReturnItem> SalesReturnItems => Set<SalesReturnItem>();

        public DbSet<SalesPayment> SalesPayments => Set<SalesPayment>();

        public DbSet<SalesQuotation> SalesQuotations => Set<SalesQuotation>();

        #endregion

        #region Purchases

        public DbSet<PurchaseInvoice> PurchaseInvoices => Set<PurchaseInvoice>();

        public DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems => Set<PurchaseInvoiceItem>();

        public DbSet<PurchaseReturn> PurchaseReturns => Set<PurchaseReturn>();

        public DbSet<PurchaseReturnItem> PurchaseReturnItems => Set<PurchaseReturnItem>();

        public DbSet<PurchasePayment> PurchasePayments => Set<PurchasePayment>();

        #endregion

        #region Accounting

        public DbSet<Cashbox> Cashboxes => Set<Cashbox>();

        public DbSet<CashTransaction> CashTransactions => Set<CashTransaction>();

        public DbSet<CashSession> CashSessions => Set<CashSession>();

        public DbSet<Expense> Expenses => Set<Expense>();

        public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();

        public DbSet<Currency> Currencies => Set<Currency>();

        public DbSet<Tax> Taxes => Set<Tax>();

        public DbSet<FinancialPeriod> FinancialPeriods => Set<FinancialPeriod>();

        #endregion

        #region Licensing

        public DbSet<License> Licenses => Set<License>();

        public DbSet<LicensePlan> LicensePlans => Set<LicensePlan>();

        public DbSet<DeviceActivation> DeviceActivations => Set<DeviceActivation>();

        #endregion

        #region Systems

        public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        public DbSet<ErrorLog> ErrorLogs => Set<ErrorLog>();

        public DbSet<AppSetting> AppSettings => Set<AppSetting>();

        public DbSet<Notification> Notifications => Set<Notification>();

        public DbSet<Attachment> Attachments => Set<Attachment>();

        public DbSet<Sequence> Sequences => Set<Sequence>();

        public DbSet<DocumentSequence> DocumentSequences => Set<DocumentSequence>();

        #endregion

        #region Reports

        public DbSet<DashboardWidget> DashboardWidgets => Set<DashboardWidget>();

        public DbSet<ReportTemplate> ReportTemplates => Set<ReportTemplate>();

        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Owned<DzentoPOS2.Core.ValueObjects.Identity.Email>();

            modelBuilder.Owned<DzentoPOS2.Core.ValueObjects.Identity.PhoneNumber>();

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var method = typeof(AppDbContext)
                        .GetMethod(nameof(SetSoftDeleteFilter),
                            System.Reflection.BindingFlags.NonPublic |
                            System.Reflection.BindingFlags.Static)
                        ?.MakeGenericMethod(entityType.ClrType);

                    method?.Invoke(null, new object[] { modelBuilder });
                }
            }

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(AppDbContext).Assembly);
        }

        private static void SetSoftDeleteFilter<TEntity>(
            ModelBuilder builder)
            where TEntity : BaseEntity
        {
            builder.Entity<TEntity>()
                .HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
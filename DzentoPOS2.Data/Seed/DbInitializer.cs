using DzentoPOS2.Data.Context;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DzentoPOS2.Data.Seed
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(
            IServiceProvider serviceProvider)
        {
            using var scope =
                serviceProvider.CreateScope();

            var context =
                scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

            // Repair the physical schema before the migrations run. This makes
            // upgrades deterministic for every starting state (fresh DB, DB at
            // AddCashSessions, DB whose history still contains the removed E3
            // migration ids). It never loses data and is idempotent.
            await LegacySchemaRepair.RepairAsync(
                context.Database.GetDbConnection(),
                scope.ServiceProvider
                    .GetService<ILoggerFactory>()
                    ?.CreateLogger("DzentoPOS2.Data.Seed.LegacySchemaRepair"));

            // تطبيق الـ Migrations تلقائياً
            await context.Database.MigrateAsync();

            await PermissionSeeder.SeedAsync(context);

            await RoleSeeder.SeedAsync(context);

            await RolePermissionSeeder.SeedAsync(context);

            await AdminSeeder.SeedAsync(context);

            await AppSettingSeeder.SeedAsync(context);

            await CurrencySeeder.SeedAsync(context);

            await LicensePlanSeeder.SeedAsync(context);

            await ProductCategorySeeder.SeedAsync(context);
            await ProductUnitSeeder.SeedAsync(context);
            await ExpenseCategorySeeder.SeedAsync(context);

            // Bootstrap the store once at startup: one deterministic main branch
            // with at least one active warehouse and cashbox under it. This
            // replaces the lazy creation that used to happen inside the sale.
            var bootstrap = await StoreBootstrapper.EnsureInitializedAsync(context);
            if (bootstrap.Changed)
            {
                System.Diagnostics.Trace.TraceInformation(
                    "Store bootstrap: {0}", bootstrap);
                System.Diagnostics.Debug.WriteLine(
                    $"[StoreBootstrapper] {bootstrap}");
            }
        }
    }
}

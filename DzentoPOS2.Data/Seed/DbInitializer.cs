using DzentoPOS2.Data.Context;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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

            // تطبيق الـ Migrations تلقائياً
            await context.Database.MigrateAsync();
              
               
            await PermissionSeeder.SeedAsync(context);

            await RoleSeeder.SeedAsync(context);

            await RolePermissionSeeder.SeedAsync(context);

            await AdminSeeder.SeedAsync(context);

            await AppSettingSeeder.SeedAsync(context);

            await CurrencySeeder.SeedAsync(context); Microsoft.Data.Sqlite.SqliteException: 'SQLite Error 1: 'no such index: IX_InventoryTransactions_ProductId'.'

            await LicensePlanSeeder.SeedAsync(context);

            await ProductCategorySeeder.SeedAsync(context);
            await ProductUnitSeeder.SeedAsync(context);
            await ExpenseCategorySeeder.SeedAsync(context);
            await WarehouseSeeder.SeedAsync(context);
            await CashboxSeeder.SeedAsync(context);
        }
    }
}
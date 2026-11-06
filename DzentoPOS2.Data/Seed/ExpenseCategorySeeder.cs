using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Core.Entities.Accounting;
using DzentoPOS2.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Seed
{
    public static class ExpenseCategorySeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.ExpenseCategories.AnyAsync())
                return;

            var categories = new List<ExpenseCategory>
            {
                new() { Name = AppStrings.ExpenseRent, Description = AppStrings.ExpenseRentDescription },
                new() { Name = AppStrings.ExpenseSalaries, Description = AppStrings.ExpenseSalariesDescription },
                new() { Name = AppStrings.ExpenseBills, Description = AppStrings.ExpenseBillsDescription },
                new() { Name = AppStrings.ExpenseMaintenance, Description = AppStrings.ExpenseMaintenanceDescription },
                new() { Name = AppStrings.ExpenseTransport, Description = AppStrings.ExpenseTransportDescription },
                new() { Name = AppStrings.ExpenseMarketing, Description = AppStrings.ExpenseMarketingDescription },
                new() { Name = AppStrings.ExpenseMisc, Description = AppStrings.ExpenseMiscDescription },
            };

            await context.ExpenseCategories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }
    }
}

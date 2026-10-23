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
                new() { Name = "إيجار", Description = "إيجار المحل أو المخزن" },
                new() { Name = "رواتب", Description = "رواتب وأجور الموظفين" },
                new() { Name = "فواتير", Description = "فواتير الكهرباء والماء والاتصالات" },
                new() { Name = "صيانة", Description = "صيانة وتصليح المعدات" },
                new() { Name = "نقل", Description = "مصاريف النقل والشحن" },
                new() { Name = "تسويق", Description = "مصاريف الإعلان والتسويق" },
                new() { Name = "متنوعة", Description = "مصاريف أخرى متنوعة" },
            };

            await context.ExpenseCategories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }
    }
}

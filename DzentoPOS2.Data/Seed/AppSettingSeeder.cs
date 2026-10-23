using DzentoPOS2.Core.Entities.Systems;
using DzentoPOS2.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Seed
{
    public static class AppSettingSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            var settings = new List<AppSetting>
            {
                new() { Key = "app.name", Value = "DzentoPOS2", Group = "General" },
                new() { Key = "app.version", Value = "1.0.0", Group = "General" },
                new() { Key = "company.name", Value = "", Group = "Company" },
                new() { Key = "company.address", Value = "", Group = "Company" },
                new() { Key = "company.phone", Value = "", Group = "Company" },
                new() { Key = "company.email", Value = "", Group = "Company" },
                new() { Key = "company.tax_number", Value = "", Group = "Company" },
                new() { Key = "pos.default_customer", Value = "عميل نقدي", Group = "POS" },
                new() { Key = "pos.receipt_footer", Value = "شكراً لتسوقكم معنا", Group = "POS" },
                new() { Key = "inventory.low_stock_threshold", Value = "10", Group = "Inventory" },
                new() { Key = "inventory.allow_negative_stock", Value = "false", Group = "Inventory" },
                new() { Key = "currency.default", Value = "SAR", Group = "Currency" },
            };

            var existingKeys = await context.AppSettings
                .AsNoTracking()
                .Select(x => x.Key)
                .ToListAsync();

            var missing = settings
                .Where(x => !existingKeys.Contains(x.Key))
                .ToList();

            if (missing.Count == 0)
                return;

            await context.AppSettings.AddRangeAsync(missing);
            await context.SaveChangesAsync();
        }
    }
}

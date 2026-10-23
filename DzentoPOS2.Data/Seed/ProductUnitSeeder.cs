using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Seed
{
    public static class ProductUnitSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.ProductUnits.AnyAsync())
                return;

            var units = new List<ProductUnit>
            {
                new() { Name = "قطعة", Symbol = "قطعة", IsBaseUnit = true },
                new() { Name = "كرتونة", Symbol = "كرتونة", IsBaseUnit = false },
                new() { Name = "كيلو جرام", Symbol = "كجم", IsBaseUnit = true },
                new() { Name = "لتر", Symbol = "لتر", IsBaseUnit = true },
                new() { Name = "حبة", Symbol = "حبة", IsBaseUnit = true },
                new() { Name = "صندوق", Symbol = "صندوق", IsBaseUnit = false },
            };

            await context.ProductUnits.AddRangeAsync(units);
            await context.SaveChangesAsync();
        }
    }
}

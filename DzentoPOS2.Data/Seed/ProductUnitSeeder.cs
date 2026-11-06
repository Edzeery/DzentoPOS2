using DzentoPOS2.Localization.Resources;
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
                new() { Name = AppStrings.UnitPiece, Symbol = AppStrings.UnitPiece, IsBaseUnit = true },
                new() { Name = AppStrings.UnitCarton, Symbol = AppStrings.UnitCarton, IsBaseUnit = false },
                new() { Name = AppStrings.UnitKilogram, Symbol = AppStrings.UnitKilogramSymbol, IsBaseUnit = true },
                new() { Name = AppStrings.UnitLiter, Symbol = AppStrings.UnitLiter, IsBaseUnit = true },
                new() { Name = AppStrings.UnitItem, Symbol = AppStrings.UnitItem, IsBaseUnit = true },
                new() { Name = AppStrings.UnitBox, Symbol = AppStrings.UnitBox, IsBaseUnit = false },
            };

            await context.ProductUnits.AddRangeAsync(units);
            await context.SaveChangesAsync();
        }
    }
}

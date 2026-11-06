using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Seed
{
    public static class ProductCategorySeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.ProductCategories.AnyAsync())
                return;

            var categories = new List<ProductCategory>
            {
                new() { Name = AppStrings.CategoryFood, Description = AppStrings.CategoryFoodDescription, IsActive = true },
                new() { Name = AppStrings.CategoryBeverages, Description = AppStrings.CategoryBeveragesDescription, IsActive = true },
                new() { Name = AppStrings.CategoryCleaning, Description = AppStrings.CategoryCleaningDescription, IsActive = true },
                new() { Name = AppStrings.CategoryOffice, Description = AppStrings.CategoryOfficeDescription, IsActive = true },
                new() { Name = AppStrings.CategoryConstruction, Description = AppStrings.CategoryConstructionDescription, IsActive = true },
            };

            await context.ProductCategories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }
    }
}

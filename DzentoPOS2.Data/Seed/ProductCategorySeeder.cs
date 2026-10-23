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
                new() { Name = "مواد غذائية", Description = "المواد الغذائية والمشروبات", IsActive = true },
                new() { Name = "مشروبات", Description = "المشروبات الساخنة والباردة", IsActive = true },
                new() { Name = "منظفات", Description = "منتجات التنظيف والتعقيم", IsActive = true },
                new() { Name = "مستلزمات مكتبية", Description = "الأدوات المكتبية والقرطاسية", IsActive = true },
                new() { Name = "مواد بناء", Description = "مواد البناء والتشييد", IsActive = true },
            };

            await context.ProductCategories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }
    }
}

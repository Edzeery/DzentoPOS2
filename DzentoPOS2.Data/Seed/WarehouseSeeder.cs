using DzentoPOS2.Core.Entities.Businesses;
using DzentoPOS2.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Seed
{
    public static class WarehouseSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.Warehouses.AnyAsync())
                return;

            var mainBranch = await context.Branches.FirstOrDefaultAsync();
            if (mainBranch == null)
            {
                mainBranch = new Branch
                {
                    Name = "الفرع الرئيسي",
                    IsActive = true
                };
                context.Branches.Add(mainBranch);
                await context.SaveChangesAsync();
            }

            var warehouses = new List<Warehouse>
            {
                new() { Name = "المخزن الرئيسي", Location = "المقر الرئيسي", BranchId = mainBranch.Id, IsActive = true },
                new() { Name = "مخزن العرض", Location = "صالة العرض", BranchId = mainBranch.Id, IsActive = true },
            };

            await context.Warehouses.AddRangeAsync(warehouses);
            await context.SaveChangesAsync();
        }
    }
}

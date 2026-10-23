using DzentoPOS2.Core.Entities.Accounting;
using DzentoPOS2.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Seed
{
    public static class CashboxSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.Cashboxes.AnyAsync())
                return;

            var mainBranch = await context.Branches.FirstOrDefaultAsync();
            if (mainBranch == null)
                return;

            var cashbox = new Cashbox
            {
                Name = "الصندوق الرئيسي",
                BranchId = mainBranch.Id,
                CurrentBalance = 0m,
                IsActive = true
            };

            await context.Cashboxes.AddAsync(cashbox);
            await context.SaveChangesAsync();
        }
    }
}

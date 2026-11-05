using DzentoPOS2.Core.Entities.Partners;
using DzentoPOS2.Data.Context;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Seed
{
    public static class CustomerAccountSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (!await context.Customers.AnyAsync(x => x.FullName == "حساب مدين"))
            {
                await context.Customers.AddAsync(new Customer
                {
                    FullName = "حساب مدين",
                    Balance = 1000m,
                    IsActive = true
                });
            }

            if (!await context.Customers.AnyAsync(x => x.FullName == "حساب دائن"))
            {
                await context.Customers.AddAsync(new Customer
                {
                    FullName = "حساب دائن",
                    Balance = -500m,
                    IsActive = true
                });
            }

            await context.SaveChangesAsync();
        }
    }
}
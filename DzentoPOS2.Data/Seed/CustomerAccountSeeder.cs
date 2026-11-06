using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Core.Entities.Partners;
using DzentoPOS2.Data.Context;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Seed
{
    public static class CustomerAccountSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (!await context.Customers.AnyAsync(x => x.FullName == AppStrings.DebitAccountCustomer))
            {
                await context.Customers.AddAsync(new Customer
                {
                    FullName = AppStrings.DebitAccountCustomer,
                    Balance = 1000m,
                    IsActive = true
                });
            }

            if (!await context.Customers.AnyAsync(x => x.FullName == AppStrings.CreditAccountCustomer))
            {
                await context.Customers.AddAsync(new Customer
                {
                    FullName = AppStrings.CreditAccountCustomer,
                    Balance = -500m,
                    IsActive = true
                });
            }

            await context.SaveChangesAsync();
        }
    }
}
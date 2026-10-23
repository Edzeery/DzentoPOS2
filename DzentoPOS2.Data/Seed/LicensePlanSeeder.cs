using DzentoPOS2.Core.Entities.Licensing;
using DzentoPOS2.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Seed
{
    public static class LicensePlanSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.LicensePlans.AnyAsync())
                return;

            var plans = new List<LicensePlan>
            {
                new()
                {
                    Name = "Basic",
                    Price = 0m,
                    DurationInDays = 30,
                    MaxUsers = 1,
                    MaxBranches = 1,
                    IsActive = true
                },
                new()
                {
                    Name = "Standard",
                    Price = 199m,
                    DurationInDays = 365,
                    MaxUsers = 3,
                    MaxBranches = 1,
                    IsActive = true
                },
                new()
                {
                    Name = "Professional",
                    Price = 499m,
                    DurationInDays = 365,
                    MaxUsers = 10,
                    MaxBranches = 3,
                    IsActive = true
                },
                new()
                {
                    Name = "Enterprise",
                    Price = 999m,
                    DurationInDays = 365,
                    MaxUsers = 999,
                    MaxBranches = 99,
                    IsActive = true
                },
            };

            await context.LicensePlans.AddRangeAsync(plans);
            await context.SaveChangesAsync();
        }
    }
}

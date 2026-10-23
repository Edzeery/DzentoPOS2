using DzentoPOS2.Core.Entities.Identity;
using DzentoPOS2.Data.Context;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Seed
{
    public static class RoleSeeder
    {
        public static async Task SeedAsync(
            AppDbContext context)
        {
            if (await context.Roles.AnyAsync())
                return;

            var roles = new List<Role>
            {
                new()
                {
                    Name = "SuperAdmin",
                    Description = "System Super Administrator"
                },

                new()
                {
                    Name = "Manager",
                    Description = "Branch Manager"
                },

                new()
                {
                    Name = "Cashier",
                    Description = "Cashier User"
                }
            };

            await context.Roles.AddRangeAsync(roles);

            await context.SaveChangesAsync();
        }
    }
}
using DzentoPOS2.Core.Entities.Identity;
using DzentoPOS2.Core.ValueObjects.Identity;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Shared.Helpers;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Seed
{
    public static class CashierSeeder
    {
        private const string DefaultUserName = "cashier";
        private const string DefaultPassword = "cashier";

        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.Users.AnyAsync(x => x.UserName == DefaultUserName))
                return;

            var cashierRole = await context.Roles.FirstOrDefaultAsync(x => x.Name == "Cashier");
            if (cashierRole == null)
                return;

            var branch = await context.Branches.FirstOrDefaultAsync();
            if (branch == null)
                return;

            var user = new User
            {
                FullName = "Cashier",
                UserName = DefaultUserName,
                Email = new Email("cashier@dzentopos.local"),
                PhoneNumber = new PhoneNumber("0600000001"),
                PasswordHash = PasswordHelper.Hash(DefaultPassword),
                BranchId = branch.Id,
                IsActive = true,
                MustChangePassword = true
            };

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            await context.UserRoles.AddAsync(new UserRole
            {
                UserId = user.Id,
                RoleId = cashierRole.Id
            });
            await context.SaveChangesAsync();
        }
    }
}
using DzentoPOS2.Core.Entities.Identity;
using DzentoPOS2.Core.ValueObjects.Identity;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Shared.Helpers;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Seed
{
    public static class AdminSeeder
    {
        private const string DefaultUserName = "admin";

        // The built-in administrator password. Kept out of the UI layer (the
        // login form no longer pre-fills it); it only exists as the seed value.
        private const string DefaultPassword = "admin";

        public static async Task SeedAsync(AppDbContext context)
        {
            var adminUser = await context.Users
                .FirstOrDefaultAsync(x =>
                    x.UserName == DefaultUserName);

            if (adminUser != null)
            {
                // Never overwrite the administrator password on every startup.
                // The password may be changed by an authorized administrator
                // and must remain exactly as stored.
                return;
            }

            var superAdminRole = await context.Roles
                .FirstOrDefaultAsync(x =>
                    x.Name == "SuperAdmin");

            if (superAdminRole == null)
                return;

            adminUser = new User
            {
                FullName = "System Administrator",
                UserName = DefaultUserName,

                Email = new Email("admin@dzentopos.local"),

                PhoneNumber = new PhoneNumber("0600000000"),

                PasswordHash = PasswordHelper.Hash(DefaultPassword),

                IsActive = true
            };

            await context.Users.AddAsync(adminUser);

            await context.SaveChangesAsync();

            await context.UserRoles.AddAsync(
                new UserRole
                {
                    UserId = adminUser.Id,
                    RoleId = superAdminRole.Id
                });

            await context.SaveChangesAsync();
        }
    }
}
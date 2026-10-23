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
                await EnsureDefaultPasswordAsync(context, adminUser);
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

        /// <summary>
        /// Keeps the built-in administrator able to sign in with the built-in
        /// default credentials. The stored hash is only rewritten when it no
        /// longer matches the default, so a database seeded with an older
        /// default is migrated automatically and <c>admin</c> / <c>admin</c>
        /// always works.
        /// </summary>
        private static async Task EnsureDefaultPasswordAsync(AppDbContext context, User adminUser)
        {
            if (PasswordHelper.Verify(DefaultPassword, adminUser.PasswordHash)) return;

            adminUser.PasswordHash = PasswordHelper.Hash(DefaultPassword);
            await context.SaveChangesAsync();
        }
    }
}
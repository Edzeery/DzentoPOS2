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
                //
                // But an account that still uses the well-known seeded password
                // keeps the default credentials working, so it is flagged for a
                // forced change on the next sign-in.
                if (!adminUser.MustChangePassword &&
                    PasswordHelper.Verify(DefaultPassword, adminUser.PasswordHash))
                {
                    adminUser.MustChangePassword = true;
                    await context.SaveChangesAsync();
                }

                // Ensure admin has a branch (fallback to main branch).
                if (adminUser.BranchId == null || adminUser.BranchId == Guid.Empty)
                {
                    var mainBranch = await context.Branches
                        .Where(b => b.IsMainBranch && b.IsActive && !b.IsDeleted)
                        .OrderBy(b => b.CreatedAt)
                        .FirstOrDefaultAsync();
                    if (mainBranch != null)
                    {
                        adminUser.BranchId = mainBranch.Id;
                        await context.SaveChangesAsync();
                    }
                }

                return;
            }

            var superAdminRole = await context.Roles
                .FirstOrDefaultAsync(x =>
                    x.Name == "SuperAdmin");

            if (superAdminRole == null)
                return;

            // Get main branch for admin
            var mainBranchForAdmin = await context.Branches
                .Where(b => b.IsMainBranch && b.IsActive && !b.IsDeleted)
                .OrderBy(b => b.CreatedAt)
                .FirstOrDefaultAsync();

            adminUser = new User
            {
                FullName = "System Administrator",
                UserName = DefaultUserName,

                Email = new Email("admin@dzentopos.local"),

                PhoneNumber = new PhoneNumber("0600000000"),

                PasswordHash = PasswordHelper.Hash(DefaultPassword),

                IsActive = true,

                BranchId = mainBranchForAdmin?.Id,

                // The seeded password is well known: the first sign-in must set
                // a real one before the shell opens.
                MustChangePassword = true
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
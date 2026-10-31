using DzentoPOS2.Core.Entities.Identity;
using DzentoPOS2.Core.ValueObjects.Identity;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Shared.Helpers;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DzentoPOS2.Data.Seed
{
    public static class AdminSeeder
    {
        private const string DefaultUserName = "admin";

        // The built-in administrator password. Kept out of the UI layer (the
        // login form no longer pre-fills it); it only exists as the seed value.
        private const string DefaultPassword = "admin";

        /// <summary>
        /// Configuration key / environment variable that triggers an emergency
        /// reset of the built-in administrator password. Set to "true" (or "1")
        /// when the administrator password is forgotten: the admin password is
        /// reset to the default and <c>MustChangePassword</c> is set, so the
        /// next sign-in forces a password change before the shell opens.
        /// </summary>
        private const string EmergencyResetKey = "Admin:EmergencyReset";

        public static async Task SeedAsync(
            AppDbContext context,
            IConfiguration? configuration = null,
            ILogger? logger = null)
        {
            var adminUser = await context.Users
                .FirstOrDefaultAsync(x =>
                    x.UserName == DefaultUserName);

            if (adminUser != null)
            {
                // Emergency reset: the administrator forgot the password. The
                // flag is read from configuration (AppSetting or environment
                // variable). After the reset the admin signs in with the
                // default password and is forced to set a new one.
                var emergencyReset = IsEmergencyResetEnabled(configuration);
                if (emergencyReset)
                {
                    adminUser.PasswordHash = PasswordHelper.Hash(DefaultPassword);
                    adminUser.MustChangePassword = true;
                    await context.SaveChangesAsync();

                    logger?.LogWarning(
                        "AdminEmergencyReset: password for {UserName} has been reset to the default. " +
                        "The user must change it on the next sign-in.",
                        DefaultUserName);

                    return;
                }

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

                IsActive = true,

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

        private static bool IsEmergencyResetEnabled(IConfiguration? configuration)
        {
            // 1. Environment variable (highest priority)
            var envValue = Environment.GetEnvironmentVariable("ADMIN_EMERGENCY_RESET");
            if (!string.IsNullOrWhiteSpace(envValue))
            {
                return string.Equals(envValue, "true", StringComparison.OrdinalIgnoreCase)
                    || envValue == "1";
            }

            // 2. Configuration key (appsettings.json or command-line override)
            var configValue = configuration?[EmergencyResetKey];
            if (!string.IsNullOrWhiteSpace(configValue))
            {
                return string.Equals(configValue, "true", StringComparison.OrdinalIgnoreCase)
                    || configValue == "1";
            }

            // 3. Sentinel file in the application directory (simplest for
            //    non-technical users): create an empty file named
            //    "reset-admin-password.txt" next to the executable and restart.
            try
            {
                var sentinelPath = Path.Combine(
                    AppContext.BaseDirectory,
                    "reset-admin-password.txt");

                if (File.Exists(sentinelPath))
                {
                    // Consume the sentinel so the reset happens only once.
                    File.Delete(sentinelPath);
                    return true;
                }
            }
            catch
            {
                // File-system errors must never block startup.
            }

            return false;
        }
    }
}
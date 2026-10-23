using DzentoPOS2.Core.Constants;
using DzentoPOS2.Core.Entities.Identity;
using DzentoPOS2.Data.Context;

using Microsoft.EntityFrameworkCore;

using System.Reflection;

namespace DzentoPOS2.Data.Seed
{
    public static class PermissionSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            var permissionFields = typeof(Permissions)
                .GetFields(BindingFlags.Public | BindingFlags.Static);

            var existingPermissions = await context.Permissions
                .Select(x => x.Code)
                .ToListAsync();

            var permissionsToAdd = new List<Permission>();

            foreach (var field in permissionFields)
            {
                var permissionCode = field.GetValue(null)?.ToString();

                if (string.IsNullOrWhiteSpace(permissionCode))
                    continue;

                if (existingPermissions.Contains(permissionCode))
                    continue;

                var module = permissionCode
                    .Split('.')[1];

                permissionsToAdd.Add(new Permission
                {
                    Name = field.Name,
                    Code = permissionCode,
                    Module = module,
                    Description = field.Name
                });
            }

            if (permissionsToAdd.Any())
            {
                await context.Permissions.AddRangeAsync(
                    permissionsToAdd);

                await context.SaveChangesAsync();
            }
        }
    }
}
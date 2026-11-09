using DzentoPOS2.Core.Constants;
using DzentoPOS2.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Seed;

public static class BranchPermissionSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Find SuperAdmin role
        var superAdminRole = await context.Roles
            .FirstOrDefaultAsync(r => r.Name == "SuperAdmin" || r.Name == "Admin");

        if (superAdminRole == null) return;

        var permissions = new[]
        {
            Permissions.BranchesSwitch,
            Permissions.BranchesViewAll,
            Permissions.BranchesActivitiesEdit
        };

        foreach (var perm in permissions)
        {
            var permission = await context.Permissions
                .FirstOrDefaultAsync(p => p.Code == perm);

            if (permission == null)
            {
                permission = new DzentoPOS2.Core.Entities.Identity.Permission
                {
                    Name = perm,
                    Code = perm,
                    Description = perm
                };
                await context.Permissions.AddAsync(permission);
                await context.SaveChangesAsync();
            }

            // Check if role already has this permission
            var hasPermission = await context.RolePermissions
                .AnyAsync(rp => rp.RoleId == superAdminRole.Id && rp.PermissionId == permission.Id);

            if (!hasPermission)
            {
                await context.RolePermissions.AddAsync(new DzentoPOS2.Core.Entities.Identity.RolePermission
                {
                    RoleId = superAdminRole.Id,
                    PermissionId = permission.Id
                });
                await context.SaveChangesAsync();
            }
        }
    }
}

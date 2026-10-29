using DzentoPOS2.Core.Constants;
using DzentoPOS2.Core.Entities.Identity;
using DzentoPOS2.Data.Context;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Seed
{
    public static class RolePermissionSeeder
    {
        public static async Task SeedAsync(
            AppDbContext context)
        {
            var superAdminRole = await context.Roles
                .FirstOrDefaultAsync(x =>
                    x.Name == "SuperAdmin");

            var managerRole = await context.Roles
                .FirstOrDefaultAsync(x =>
                    x.Name == "Manager");

            var cashierRole = await context.Roles
                .FirstOrDefaultAsync(x =>
                    x.Name == "Cashier");

            if (superAdminRole is null ||
                managerRole is null ||
                cashierRole is null)
            {
                return;
            }

            var permissions = await context.Permissions
                .ToListAsync();

            // SUPER ADMIN
            await AssignPermissionsAsync(
                context,
                superAdminRole.Id,
                permissions.Select(x => x.Code).ToList());

            // MANAGER
            await AssignPermissionsAsync(
                context,
                managerRole.Id,
                new List<string>
                {
                    Permissions.DashboardView,

                    Permissions.ProductsView,
                    Permissions.ProductsCreate,
                    Permissions.ProductsEdit,

                    Permissions.CategoriesView,
                    Permissions.UnitsView,

                    Permissions.InventoryView,
                    Permissions.InventoryAdjust,

                    Permissions.SalesView,
                    Permissions.SalesCreate,
                    Permissions.SalesReturn,

                    Permissions.CustomersView,
                    Permissions.CustomersCreate,

                    Permissions.SuppliersView,
                    Permissions.SuppliersCreate,
                    Permissions.SuppliersEdit,

                    Permissions.PurchasesView,
                    Permissions.PurchasesCreate,

                    Permissions.ExpensesView,
                    Permissions.ExpensesCreate,

                    Permissions.CashSessionsView,

                    Permissions.ReportsView,
                    Permissions.SettingsView
                });

            // CASHIER
            await AssignPermissionsAsync(
                context,
                cashierRole.Id,
                new List<string>
                {
                    Permissions.DashboardView,

                    Permissions.SalesView,
                    Permissions.SalesCreate,

                    Permissions.CustomersView,
                    Permissions.CustomersCreate,

                    Permissions.ProductsView,
                    Permissions.CashSessionsView
                });
        }

        private static async Task AssignPermissionsAsync(
            AppDbContext context,
            Guid roleId,
            List<string> permissionCodes)
        {
            var permissions = await context.Permissions
                .Where(x => permissionCodes.Contains(x.Code))
                .ToListAsync();

            var existingPermissions =
                await context.RolePermissions
                    .Where(x => x.RoleId == roleId)
                    .Select(x => x.PermissionId)
                    .ToListAsync();

            var newRolePermissions =
                permissions
                    .Where(x => !existingPermissions.Contains(x.Id))
                    .Select(x => new RolePermission
                    {
                        RoleId = roleId,
                        PermissionId = x.Id
                    })
                    .ToList();

            if (newRolePermissions.Any())
            {
                await context.RolePermissions
                    .AddRangeAsync(newRolePermissions);

                await context.SaveChangesAsync();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DzentoPOS2.Core.Constants
{
    public static class Permissions
    {
        // Dashboard
        public const string DashboardView = "permissions.dashboard.view";

        // Users
        public const string UsersView = "permissions.users.view";
        public const string UsersCreate = "permissions.users.create";
        public const string UsersEdit = "permissions.users.edit";
        public const string UsersDelete = "permissions.users.delete";

        // Roles
        public const string RolesView = "permissions.roles.view";
        public const string RolesCreate = "permissions.roles.create";
        public const string RolesEdit = "permissions.roles.edit";
        public const string RolesDelete = "permissions.roles.delete";

        // Products
        public const string ProductsView = "permissions.products.view";
        public const string ProductsCreate = "permissions.products.create";
        public const string ProductsEdit = "permissions.products.edit";
        public const string ProductsDelete = "permissions.products.delete";

        // Categories
        public const string CategoriesView = "permissions.categories.view";
        public const string CategoriesCreate = "permissions.categories.create";
        public const string CategoriesEdit = "permissions.categories.edit";
        public const string CategoriesDelete = "permissions.categories.delete";

        // Inventory
        public const string InventoryView = "permissions.inventory.view";
        public const string InventoryAdjust = "permissions.inventory.adjust";
        public const string InventoryTransfer = "permissions.inventory.transfer";

        // Sales
        public const string SalesView = "permissions.sales.view";
        public const string SalesCreate = "permissions.sales.create";
        public const string SalesReturn = "permissions.sales.return";
        public const string SalesDelete = "permissions.sales.delete";

        // Purchases
        public const string PurchasesView = "permissions.purchases.view";
        public const string PurchasesCreate = "permissions.purchases.create";
        public const string PurchasesReturn = "permissions.purchases.return";
        public const string PurchasesDelete = "permissions.purchases.delete";

        // Customers
        public const string CustomersView = "permissions.customers.view";
        public const string CustomersCreate = "permissions.customers.create";
        public const string CustomersEdit = "permissions.customers.edit";
        public const string CustomersDelete = "permissions.customers.delete";

        // Suppliers
        public const string SuppliersView = "permissions.suppliers.view";
        public const string SuppliersCreate = "permissions.suppliers.create";
        public const string SuppliersEdit = "permissions.suppliers.edit";
        public const string SuppliersDelete = "permissions.suppliers.delete";

        // Expenses
        public const string ExpensesView = "permissions.expenses.view";
        public const string ExpensesCreate = "permissions.expenses.create";
        public const string ExpensesEdit = "permissions.expenses.edit";
        public const string ExpensesDelete = "permissions.expenses.delete";

        // Reports
        public const string ReportsView = "permissions.reports.view";

        // Settings
        public const string SettingsView = "permissions.settings.view";
        public const string SettingsEdit = "permissions.settings.edit";
    }
}
using DzentoPOS2.App.Authentication;
using DzentoPOS2.App.Authorization;
using DzentoPOS2.App.Services.Accounting;
using DzentoPOS2.App.Services.Identity;
using DzentoPOS2.App.Services.Identity.Interfaces;
using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.App.Services.Partners;
using DzentoPOS2.App.Services.Purchases;
using DzentoPOS2.App.Services.Reports;
using DzentoPOS2.App.Services.Reports.Interfaces;
using DzentoPOS2.App.Services.Sales;
using DzentoPOS2.App.Session;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DzentoPOS2.App
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services)
        {
            services.AddAutoMapper(cfg => { },
                Assembly.GetExecutingAssembly());

            services.AddValidatorsFromAssembly(
                Assembly.GetExecutingAssembly());

            // Regional configuration (currency / country / time zone) is shared
            // by the application layer and the user interface.
            services.AddSingleton<Shared.Helpers.RegionalContext>();

            services.AddScoped<IAuthService, AuthService>();

            services.AddSingleton<ICurrentUserService,
                CurrentUserService>();

            services.AddScoped<IPermissionService,
                PermissionService>();

    services.AddScoped<IPasswordHasher,
        PasswordHasher>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<
                    IPermissionManagementService,
                    PermissionManagementService>();

            services.AddScoped<
                IDashboardService,
                DashboardService>();

            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IExpenseService, ExpenseService>();

            services.AddScoped<IProductCategoryService, ProductCategoryService>();
            services.AddScoped<IProductUnitService, ProductUnitService>();

            services.AddScoped<ISaleService, SaleService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<ISupplierService, SupplierService>();
            services.AddScoped<IPurchaseService, PurchaseService>();

            services.AddScoped<ICashSessionService, CashSessionService>();

            return services;
        }
    }
}
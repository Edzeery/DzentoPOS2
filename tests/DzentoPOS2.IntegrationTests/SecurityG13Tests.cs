using DzentoPOS2.App;
using DzentoPOS2.App.Authorization;
using DzentoPOS2.App.Services.Identity.Interfaces;
using DzentoPOS2.App.Session;
using DzentoPOS2.Core.Constants;
using DzentoPOS2.Core.Exceptions;
using DzentoPOS2.Data;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Seed;
using DzentoPOS2.Shared.Contracts.DTOs.Identity.Auth;
using DzentoPOS2.Shared.Helpers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DzentoPOS2.IntegrationTests;

/// <summary>
/// G1.3 — security hardening: forced password change, service-layer permission
/// enforcement and audit logging.
/// </summary>
public sealed class SecurityG13Tests : IAsyncLifetime
{
    private ServiceProvider _provider = null!;
    private string _dbPath = string.Empty;
    private TestData _data = null!;

    public async Task InitializeAsync()
    {
        (_provider, _dbPath) = BuildProvider();

        await using var scope = _provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();

        _data = await TestData.SeedAsync(context);
    }

    public async Task DisposeAsync()
    {
        await _provider.DisposeAsync();
        SqliteConnection.ClearAllPools();

        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
    }

    [Fact]
    public async Task Forced_password_change_is_flagged_on_login_and_cleared_after_the_change()
    {
        const string original = "Secret123!";
        const string replacement = "NewSecret456!";

        await SetPasswordAsync(original, mustChange: true);

        await using var scope = _provider.CreateAsyncScope();
        var auth = scope.ServiceProvider.GetRequiredService<IAuthService>();

        var login = await auth.LoginAsync(new LoginDto
        {
            UserName = "cashier",
            Password = original
        });

        Assert.True(login.Success);
        Assert.True(login.Data!.MustChangePassword);

        var change = await auth.ChangePasswordAsync(new ChangePasswordDto
        {
            UserId = _data.UserId,
            CurrentPassword = original,
            NewPassword = replacement,
            ConfirmPassword = replacement
        });

        Assert.True(change.Success, change.Message);

        await using var verify = _provider.CreateAsyncScope();
        var verifyContext = verify.ServiceProvider.GetRequiredService<AppDbContext>();

        var stored = await verifyContext.Users
            .AsNoTracking()
            .SingleAsync(u => u.Id == _data.UserId);

        Assert.False(stored.MustChangePassword);

        // The second sign-in no longer asks for a change.
        var relogin = await auth.LoginAsync(new LoginDto
        {
            UserName = "cashier",
            Password = replacement
        });

        Assert.True(relogin.Success);
        Assert.False(relogin.Data!.MustChangePassword);
    }

    [Fact]
    public async Task Login_and_password_change_are_written_to_the_audit_log()
    {
        const string original = "AuditPass123!";
        const string replacement = "AuditPass456!";

        await SetPasswordAsync(original, mustChange: true);

        await using var scope = _provider.CreateAsyncScope();
        var auth = scope.ServiceProvider.GetRequiredService<IAuthService>();

        await auth.LoginAsync(new LoginDto
        {
            UserName = "cashier",
            Password = original
        });

        await auth.ChangePasswordAsync(new ChangePasswordDto
        {
            UserId = _data.UserId,
            CurrentPassword = original,
            NewPassword = replacement,
            ConfirmPassword = replacement
        });

        await using var verify = _provider.CreateAsyncScope();
        var context = verify.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.True(await context.AuditLogs.AnyAsync(a => a.Action == "Login"));
        Assert.True(await context.AuditLogs.AnyAsync(a => a.Action == "ChangePassword"));
    }

    [Fact]
    public async Task Guard_denies_a_user_without_the_permission_and_writes_an_audit_entry()
    {
        await using var scope = _provider.CreateAsyncScope();

        var currentUser = scope.ServiceProvider.GetRequiredService<ICurrentUserService>();
        currentUser.SetUser(new UserSession
        {
            UserId = _data.UserId,
            UserName = "cashier",
            RoleName = "Cashier",
            IsAdmin = false
        });

        var guard = scope.ServiceProvider.GetRequiredService<IAuthorizationGuard>();

        var error = await Assert.ThrowsAsync<CodedDomainException>(
            () => guard.DemandAsync(Permissions.UsersCreate));

        Assert.Equal(DomainErrorCodes.PermissionDenied, error.Code);

        await using var verify = _provider.CreateAsyncScope();
        var context = verify.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.True(await context.AuditLogs.AnyAsync(a =>
            a.Action == "PermissionDenied" &&
            a.NewValues == Permissions.UsersCreate));
    }

    [Fact]
    public async Task Guard_allows_an_admin_account_without_a_permission_row()
    {
        await using var scope = _provider.CreateAsyncScope();

        var currentUser = scope.ServiceProvider.GetRequiredService<ICurrentUserService>();
        currentUser.SetUser(new UserSession
        {
            UserId = _data.UserId,
            UserName = "cashier",
            RoleName = "Cashier",
            IsAdmin = true
        });

        var guard = scope.ServiceProvider.GetRequiredService<IAuthorizationGuard>();

        await guard.DemandAsync(Permissions.UsersCreate);

        Assert.True(await guard.IsAllowedAsync(Permissions.UsersCreate));
    }

    [Fact]
    public async Task Guard_denies_an_unauthenticated_caller()
    {
        await using var scope = _provider.CreateAsyncScope();

        var currentUser = scope.ServiceProvider.GetRequiredService<ICurrentUserService>();
        currentUser.Clear();

        var guard = scope.ServiceProvider.GetRequiredService<IAuthorizationGuard>();

        var error = await Assert.ThrowsAsync<CodedDomainException>(
            () => guard.DemandAsync(Permissions.SalesCreate));

        Assert.Equal(DomainErrorCodes.PermissionDenied, error.Code);
    }

    [Fact]
    public async Task Seeded_admin_is_flagged_and_re_seeding_keeps_the_flag_without_duplicating_the_account()
    {
        await using var scope = _provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await RoleSeeder.SeedAsync(context);
        await AdminSeeder.SeedAsync(context);
        await AdminSeeder.SeedAsync(context);

        var admin = await context.Users
            .AsNoTracking()
            .SingleAsync(u => u.UserName == "admin");

        Assert.True(admin.MustChangePassword);
        Assert.Equal(1, await context.Users.CountAsync(u => u.UserName == "admin"));
    }

    [Fact]
    public async Task Re_seeding_does_not_force_a_change_after_the_admin_password_was_replaced()
    {
        await using var scope = _provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await RoleSeeder.SeedAsync(context);
        await AdminSeeder.SeedAsync(context);

        var admin = await context.Users.SingleAsync(u => u.UserName == "admin");
        admin.PasswordHash = PasswordHelper.Hash("RealAdminPass1!");
        admin.MustChangePassword = false;
        await context.SaveChangesAsync();

        await AdminSeeder.SeedAsync(context);

        var stored = await context.Users
            .AsNoTracking()
            .SingleAsync(u => u.UserName == "admin");

        Assert.False(stored.MustChangePassword);
        Assert.True(PasswordHelper.Verify("RealAdminPass1!", stored.PasswordHash));
    }

    private async Task SetPasswordAsync(string password, bool mustChange)
    {
        await using var scope = _provider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = await context.Users.SingleAsync(u => u.Id == _data.UserId);
        user.PasswordHash = PasswordHelper.Hash(password);
        user.MustChangePassword = mustChange;

        await context.SaveChangesAsync();
    }

    private static (ServiceProvider Provider, string DbPath) BuildProvider()
    {
        var directory = Path.Combine(Path.GetTempPath(), "DzentoPOS2.G13");
        Directory.CreateDirectory(directory);

        var dbPath = Path.Combine(directory, $"{Guid.NewGuid():N}.db");

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={dbPath}"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddDataServices(configuration);
        services.AddApplicationServices();

        return (services.BuildServiceProvider(), dbPath);
    }
}
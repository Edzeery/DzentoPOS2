using DzentoPOS2.App.Services.Licensing;
using DzentoPOS2.Core.Exceptions;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Licensing;
using DzentoPOS2.Licensing.Signing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DzentoPOS2.IntegrationTests;

[Collection(PosDatabaseCollection.Name)]
public sealed class LicenseServiceTests
{
    private readonly PosDatabaseFixture _fixture;

    public LicenseServiceTests(PosDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Unlicensed_installation_is_a_writable_trial()
    {
        await using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await ResetAsync(context);

        var service = scope.ServiceProvider.GetRequiredService<ILicenseService>();
        var status = await service.GetStatusAsync();

        Assert.Equal(LicenseState.Unlicensed, status.State);
        Assert.True(status.IsWritable);
        Assert.False(status.IsEnforced);
    }

    [Fact]
    public async Task Activation_persists_the_license_and_binds_the_device()
    {
        await using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await ResetAsync(context);

        var service = scope.ServiceProvider.GetRequiredService<ILicenseService>();
        var deviceId = DeviceId(scope.ServiceProvider);

        var key = LicenseSigner.Sign(
            BuildPayload(deviceId, maxUsers: 5, maxBranches: 2),
            _fixture.LicensePrivateKeyPem);

        var result = await service.ActivateAsync(key);
        Assert.True(result.Success);

        var status = await service.GetStatusAsync();
        Assert.Equal(LicenseState.Active, status.State);
        Assert.Equal("Standard", status.PlanName);
        Assert.Equal(5, status.MaxUsers);
        Assert.Equal(2, status.MaxBranches);
        Assert.True(status.IsWritable);

        Assert.Equal(1, await context.Licenses.CountAsync());
        Assert.Equal(1, await context.DeviceActivations.CountAsync());
        Assert.Equal(1, await context.LicensePlans.CountAsync());
    }

    [Fact]
    public async Task Activation_rejects_a_tampered_key()
    {
        await using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await ResetAsync(context);

        var service = scope.ServiceProvider.GetRequiredService<ILicenseService>();
        var deviceId = DeviceId(scope.ServiceProvider);

        var key = LicenseSigner.Sign(
            BuildPayload(deviceId, maxUsers: 5, maxBranches: 2),
            _fixture.LicensePrivateKeyPem);

        var result = await service.ActivateAsync(Tamper(key));

        Assert.False(result.Success);
        Assert.Equal(0, await context.Licenses.CountAsync());
    }

    [Fact]
    public async Task Activation_rejects_a_key_issued_for_another_device()
    {
        await using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await ResetAsync(context);

        var service = scope.ServiceProvider.GetRequiredService<ILicenseService>();

        var key = LicenseSigner.Sign(
            BuildPayload("another-device", maxUsers: 5, maxBranches: 2),
            _fixture.LicensePrivateKeyPem);

        var result = await service.ActivateAsync(key);

        Assert.False(result.Success);
        Assert.Equal(0, await context.Licenses.CountAsync());
    }

    [Fact]
    public async Task Expired_license_is_read_only_when_enforced()
    {
        await using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var options = _fixture.Services.GetRequiredService<LicenseOptions>();
        await ResetAsync(context);

        var service = scope.ServiceProvider.GetRequiredService<ILicenseService>();
        var deviceId = DeviceId(scope.ServiceProvider);

        try
        {
            options.Enforce = true;

            var key = LicenseSigner.Sign(
                BuildPayload(deviceId, maxUsers: 5, maxBranches: 2),
                _fixture.LicensePrivateKeyPem);

            Assert.True((await service.ActivateAsync(key)).Success);

            // Backdate the stored row: the token was valid at activation, the
            // licence then ran out.
            var license = await context.Licenses.FirstAsync();
            license.ExpiryDate = DateTime.UtcNow.AddDays(-30);
            await context.SaveChangesAsync();

            var status = await service.GetStatusAsync();
            Assert.Equal(LicenseState.Expired, status.State);
            Assert.False(status.IsWritable);

            var exception = await Assert.ThrowsAsync<CodedDomainException>(
                () => service.EnsureWritableAsync());
            Assert.Equal(DomainErrorCodes.LicenseExpired, exception.Code);
        }
        finally
        {
            options.Enforce = false;
        }
    }

    [Fact]
    public async Task Grace_period_stays_writable()
    {
        await using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var options = _fixture.Services.GetRequiredService<LicenseOptions>();
        await ResetAsync(context);

        var service = scope.ServiceProvider.GetRequiredService<ILicenseService>();
        var deviceId = DeviceId(scope.ServiceProvider);

        try
        {
            options.Enforce = true;

            var key = LicenseSigner.Sign(
                BuildPayload(deviceId, maxUsers: 5, maxBranches: 2),
                _fixture.LicensePrivateKeyPem);

            Assert.True((await service.ActivateAsync(key)).Success);

            var license = await context.Licenses.FirstAsync();
            license.ExpiryDate = DateTime.UtcNow.AddDays(-3);
            await context.SaveChangesAsync();

            var status = await service.GetStatusAsync();

            Assert.Equal(LicenseState.GracePeriod, status.State);
            Assert.True(status.IsWritable);
            await service.EnsureWritableAsync();
        }
        finally
        {
            options.Enforce = false;
        }
    }

    [Fact]
    public async Task Branch_limit_is_enforced_at_the_plan_ceiling()
    {
        await using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var options = _fixture.Services.GetRequiredService<LicenseOptions>();
        await ResetAsync(context);

        var service = scope.ServiceProvider.GetRequiredService<ILicenseService>();
        var deviceId = DeviceId(scope.ServiceProvider);

        try
        {
            options.Enforce = true;

            var singleBranch = LicenseSigner.Sign(
                BuildPayload(deviceId, maxUsers: 100, maxBranches: 1),
                _fixture.LicensePrivateKeyPem);
            Assert.True((await service.ActivateAsync(singleBranch)).Success);

            var exception = await Assert.ThrowsAsync<CodedDomainException>(
                () => service.EnsureCanCreateBranchAsync());
            Assert.Equal(DomainErrorCodes.LicenseBranchLimit, exception.Code);

            var multiBranch = LicenseSigner.Sign(
                BuildPayload(deviceId, maxUsers: 100, maxBranches: 100),
                _fixture.LicensePrivateKeyPem);
            Assert.True((await service.ActivateAsync(multiBranch)).Success);

            await service.EnsureCanCreateBranchAsync();
        }
        finally
        {
            options.Enforce = false;
        }
    }

    [Fact]
    public async Task User_limit_is_enforced_at_the_plan_ceiling()
    {
        await using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var options = _fixture.Services.GetRequiredService<LicenseOptions>();
        await ResetAsync(context);

        var service = scope.ServiceProvider.GetRequiredService<ILicenseService>();
        var deviceId = DeviceId(scope.ServiceProvider);

        try
        {
            options.Enforce = true;

            var singleUser = LicenseSigner.Sign(
                BuildPayload(deviceId, maxUsers: 1, maxBranches: 100),
                _fixture.LicensePrivateKeyPem);
            Assert.True((await service.ActivateAsync(singleUser)).Success);

            var exception = await Assert.ThrowsAsync<CodedDomainException>(
                () => service.EnsureCanCreateUserAsync());
            Assert.Equal(DomainErrorCodes.LicenseUserLimit, exception.Code);
        }
        finally
        {
            options.Enforce = false;
        }
    }

    [Fact]
    public async Task Deactivation_returns_the_installation_to_unlicensed()
    {
        await using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await ResetAsync(context);

        var service = scope.ServiceProvider.GetRequiredService<ILicenseService>();
        var deviceId = DeviceId(scope.ServiceProvider);

        var key = LicenseSigner.Sign(
            BuildPayload(deviceId, maxUsers: 5, maxBranches: 2),
            _fixture.LicensePrivateKeyPem);
        Assert.True((await service.ActivateAsync(key)).Success);

        var result = await service.DeactivateAsync();
        Assert.True(result.Success);

        var status = await service.GetStatusAsync();
        Assert.Equal(LicenseState.Unlicensed, status.State);
        Assert.Equal(0, await context.DeviceActivations.CountAsync(a => !a.IsDeleted));
    }

    private static string DeviceId(IServiceProvider services) =>
        services.GetRequiredService<IDeviceIdentifier>().GetDeviceId();

    private static LicensePayload BuildPayload(
        string deviceId,
        int maxUsers,
        int maxBranches,
        int expiresInDays = 365,
        string plan = "Standard") => new()
        {
            Serial = Guid.NewGuid().ToString("N"),
            Plan = plan,
            DeviceId = deviceId,
            IssuedAt = DateTime.UtcNow.AddDays(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(expiresInDays),
            MaxUsers = maxUsers,
            MaxBranches = maxBranches
        };

    private static string Tamper(string token)
    {
        var parts = token.Split('.');
        var signature = parts[1].ToCharArray();
        signature[0] = signature[0] == 'A' ? 'B' : 'A';
        return $"{parts[0]}.{new string(signature)}";
    }

    private static async Task ResetAsync(AppDbContext context)
    {
        await context.DeviceActivations.ExecuteDeleteAsync();
        await context.Licenses.ExecuteDeleteAsync();
        await context.LicensePlans.ExecuteDeleteAsync();
    }
}
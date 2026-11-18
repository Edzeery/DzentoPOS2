using DzentoPOS2.App;
using DzentoPOS2.App.Services.Licensing;
using DzentoPOS2.App.Session;
using DzentoPOS2.Data;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Licensing;
using DzentoPOS2.Licensing.Signing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DzentoPOS2.IntegrationTests;

public sealed class PosDatabaseFixture : IAsyncLifetime
{
    private readonly string _directory = Path.Combine(
        Path.GetTempPath(),
        "DzentoPOS2.Tests");

    public string DatabasePath { get; private set; } = string.Empty;

    public ServiceProvider Services { get; private set; } = null!;

    public TestData Data { get; private set; } = null!;

    /// <summary>
    /// Test-only vendor key pair. Tests register a <see cref="LicenseOptions"/>
    /// holding the public half before the application services are added; the
    /// private half is used to issue signed keys.
    /// </summary>
    public string LicensePrivateKeyPem { get; private set; } = string.Empty;

    public string LicensePublicKeyPem { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        Directory.CreateDirectory(_directory);

        DatabasePath = Path.Combine(
            _directory,
            $"{Guid.NewGuid():N}.db");

        (LicensePrivateKeyPem, LicensePublicKeyPem) =
            LicenseSigner.GenerateKeyPair();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] =
                    $"Data Source={DatabasePath}"
            })
            .Build();

        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddDataServices(configuration);

        // Inject the test key pair: the customer application embeds the vendor
        // public key in code, so a test key can only be supplied here.
        services.AddSingleton(new LicenseOptions
        {
            PublicKeyPem = LicensePublicKeyPem
        });

        services.AddApplicationServices();

        Services = services.BuildServiceProvider();

        await using var scope = Services.CreateAsyncScope();

        var context = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        await context.Database.MigrateAsync();

        Data = await TestData.SeedAsync(context);

        var currentUser = scope.ServiceProvider
            .GetRequiredService<ICurrentUserService>();

        currentUser.SetUser(new UserSession
        {
            UserId = Data.UserId,
            UserName = "cashier",
            FullName = "Test Cashier",
            RoleName = "Cashier",
            BranchId = Data.BranchId,
            BranchName = "Main Branch",
            IsAdmin = true
        });

        var activeBranch = scope.ServiceProvider
            .GetRequiredService<DzentoPOS2.App.Services.Systems.IActiveBranchContext>();
        await activeBranch.InitializeAsync(currentUser);
    }

    public AsyncServiceScope CreateScope()
    {
        return Services.CreateAsyncScope();
    }

    public async Task DisposeAsync()
    {
        await Services.DisposeAsync();

        SqliteConnection.ClearAllPools();

        if (File.Exists(DatabasePath))
            File.Delete(DatabasePath);
    }
}

[CollectionDefinition(Name)]
public sealed class PosDatabaseCollection
    : ICollectionFixture<PosDatabaseFixture>
{
    public const string Name = "PosDatabase";
}

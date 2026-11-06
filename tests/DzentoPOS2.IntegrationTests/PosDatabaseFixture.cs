using DzentoPOS2.App;
using DzentoPOS2.App.Session;
using DzentoPOS2.Data;
using DzentoPOS2.Data.Context;
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

    public async Task InitializeAsync()
    {
        Directory.CreateDirectory(_directory);

        DatabasePath = Path.Combine(
            _directory,
            $"{Guid.NewGuid():N}.db");

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

using DzentoPOS2.Data.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
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

    public async Task InitializeAsync()
    {
        Directory.CreateDirectory(_directory);

        DatabasePath = Path.Combine(
            _directory,
            $"{Guid.NewGuid():N}.db");

        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite($"Data Source={DatabasePath}"));

        Services = services.BuildServiceProvider();

        await using var scope = Services.CreateAsyncScope();

        var context = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        await context.Database.MigrateAsync();
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

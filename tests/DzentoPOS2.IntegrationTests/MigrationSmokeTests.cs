using DzentoPOS2.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DzentoPOS2.IntegrationTests;

[Collection(PosDatabaseCollection.Name)]
public sealed class MigrationSmokeTests
{
    private readonly PosDatabaseFixture _fixture;

    public MigrationSmokeTests(PosDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task MigrationsCreateSqliteDatabase()
    {
        await using var scope = _fixture.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        Assert.True(await context.Database.CanConnectAsync());

        var applied = await context.Database
            .GetAppliedMigrationsAsync();

        Assert.NotEmpty(applied);
    }
}

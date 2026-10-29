using DzentoPOS2.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
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

    [Fact]
    public async Task Model_matches_the_last_migration_snapshot()
    {
        // Equivalent of `dotnet ef migrations has-pending-model-changes`
        // (the CLI cannot run while the repo has two design-time factories for
        // the same context), so CI still catches drift between the model and
        // the migration snapshot.
        await using var scope = _fixture.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var snapshotModel = context.GetService<IMigrationsAssembly>().ModelSnapshot?.Model;

        Assert.NotNull(snapshotModel);

        // Mirror what `dotnet ef migrations has-pending-model-changes` does:
        // finalize and runtime-initialize the snapshot model before comparing.
        if (snapshotModel is IMutableModel mutableModel)
            snapshotModel = mutableModel.FinalizeModel();

        snapshotModel = context
            .GetService<IModelRuntimeInitializer>()
            .Initialize(snapshotModel!, designTime: true);

        var designModel = context.GetService<IDesignTimeModel>().Model;

        var hasDifferences = context
            .GetService<IMigrationsModelDiffer>()
            .HasDifferences(
                snapshotModel.GetRelationalModel(),
                designModel.GetRelationalModel());

        Assert.False(hasDifferences, "The EF model has changes not captured by a migration.");
    }
}

using DzentoPOS2.App;
using DzentoPOS2.Data;
using DzentoPOS2.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DzentoPOS2.IntegrationTests;

/// <summary>
/// Guards the G1.2 invariant: a data context must never be shared for the
/// whole session. The DI graph has to be scope-valid, and both
/// <see cref="AppDbContext"/> and <see cref="IDbContextFactory{TContext}"/>
/// must be resolved per scope (page), not once for the application.
/// </summary>
public sealed class ScopeLifetimeTests
{
    private static ServiceProvider BuildProvider()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            "DzentoPOS2.Tests",
            $"scope-{Guid.NewGuid():N}.db");

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] =
                    $"Data Source={databasePath}"
            })
            .Build();

        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddDataServices(configuration);
        services.AddApplicationServices();

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });
    }

    [Fact]
    public void ServiceProvider_BuildsWithScopeValidationEnabled()
    {
        // Throws if any singleton captures a scoped dependency, which is
        // exactly the "one DbContext for the whole session" bug.
        using var provider = BuildProvider();

        Assert.NotNull(provider);
    }

    [Fact]
    public void AppDbContext_IsScoped_NotSharedBetweenScopes()
    {
        using var provider = BuildProvider();

        using var scopeA = provider.CreateScope();
        using var scopeB = provider.CreateScope();

        var first = scopeA.ServiceProvider.GetRequiredService<AppDbContext>();
        var second = scopeA.ServiceProvider.GetRequiredService<AppDbContext>();
        var other = scopeB.ServiceProvider.GetRequiredService<AppDbContext>();

        // One context per scope...
        Assert.Same(first, second);

        // ...and a different one for every other scope (page).
        Assert.NotSame(first, other);
    }

    [Fact]
    public void ContextFactory_IsScoped_NotSharedBetweenScopes()
    {
        using var provider = BuildProvider();

        using var scopeA = provider.CreateScope();
        using var scopeB = provider.CreateScope();

        var factoryA = scopeA.ServiceProvider
            .GetRequiredService<IDbContextFactory<AppDbContext>>();
        var factoryB = scopeB.ServiceProvider
            .GetRequiredService<IDbContextFactory<AppDbContext>>();

        Assert.NotSame(factoryA, factoryB);

        // A factory always hands out a fresh, short-lived context.
        using var context1 = factoryA.CreateDbContext();
        using var context2 = factoryA.CreateDbContext();

        Assert.NotSame(context1, context2);
    }
}

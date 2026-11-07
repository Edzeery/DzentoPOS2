using DzentoPOS2.Core.Common;
using DzentoPOS2.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace DzentoPOS2.Data.Interceptors;

/// <summary>
/// Fills <see cref="AuditableEntity.CreatedBy"/> / <see cref="AuditableEntity.UpdatedBy"/>
/// from the signed-in user when they are still null. Existing (legacy) values are
/// never overwritten, so rows with null ownership stay "unspecified".
/// </summary>
public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    public static readonly AuditSaveChangesInterceptor Instance = new();

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        Apply(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Apply(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void Apply(DbContext? context)
    {
        if (context is null) return;

        var sp = (context as Microsoft.EntityFrameworkCore.Infrastructure.IInfrastructure<IServiceProvider>)?.Instance;
        var accessor = sp?.GetService<ICurrentUserAccessor>();
        var userId = accessor?.UserId;
        if (userId is null) return;

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.CreatedBy is null)
                    entry.Entity.CreatedBy = userId;
                if (entry.Entity.UpdatedBy is null)
                    entry.Entity.UpdatedBy = userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                if (entry.Entity.UpdatedBy is null)
                    entry.Entity.UpdatedBy = userId;
            }
        }
    }
}
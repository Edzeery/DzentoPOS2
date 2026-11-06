using DzentoPOS2.Core.Entities.Systems;
using DzentoPOS2.Data.Context;
using DzentoPOS2.App.Session;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DzentoPOS2.App.Authorization;

/// <summary>
/// Writes security-relevant events (login, logout, password change, privilege
/// bypass/denial) to the <c>AuditLogs</c> table.
/// </summary>
public interface IAuditService
{
    Task LogAsync(
        string action,
        string entityName,
        string oldValues = "",
        string newValues = "",
        Guid? userId = null);
}

/// <summary>
/// Audit writer that owns its own short-lived <see cref="AppDbContext"/>, so an
/// audit entry never commits whatever the caller's unit of work still has
/// pending, and a failed audit write never breaks the operation itself.
/// </summary>
public sealed class AuditService : IAuditService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        IDbContextFactory<AppDbContext> contextFactory,
        ICurrentUserService currentUser,
        ILogger<AuditService> logger)
    {
        _contextFactory = contextFactory;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task LogAsync(
        string action,
        string entityName,
        string oldValues = "",
        string newValues = "",
        Guid? userId = null)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            context.AuditLogs.Add(new AuditLog
            {
                UserId = userId ?? _currentUser.UserId ?? Guid.Empty,
                Action = action,
                EntityName = entityName,
                OldValues = oldValues,
                NewValues = newValues,
                IpAddress = string.Empty
            });

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Auditing is observability: record the failure, never rethrow.
            _logger.LogWarning(
                ex,
                "Failed to write audit entry {Action} for {EntityName}.",
                action,
                entityName);
        }
    }
}
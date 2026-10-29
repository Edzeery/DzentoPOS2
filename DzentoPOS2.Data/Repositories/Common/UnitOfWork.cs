using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common.Interfaces;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace DzentoPOS2.Data.Repositories.Common
{
    public class UnitOfWork : IUnitOfWork, IAsyncDisposable
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UnitOfWork> _logger;

        private IDbContextTransaction? _transaction;

        public UnitOfWork(AppDbContext context, ILogger<UnitOfWork> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> SaveChangesAsync()
        {
            // The application resolves the DbContext from the root provider, so
            // a single context (and change tracker) lives for the whole session.
            // A failed save would otherwise leave the offending entities tracked
            // and poison every later save until the app is restarted. Clearing
            // the tracker after a failure is an interim safety net until the E4
            // scoped-context work lands.
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                LogTrackedEntries(ex);
                _context.ChangeTracker.Clear();
                throw;
            }
        }

        private void LogTrackedEntries(DbUpdateException ex)
        {
            _logger.LogError(
                ex,
                "SaveChanges failed ({Kind}). Clearing the change tracker. Tracked entries: {Entries}",
                ex.GetType().Name,
                DescribeEntries(ex));
        }

        private static string DescribeEntries(DbUpdateException ex)
        {
            if (ex.Entries.Count == 0)
                return "<none>";

            return string.Join(
                ", ",
                ex.Entries.Select(e => $"{e.Metadata.ClrType.Name}:{e.State}"));
        }

        public async Task BeginTransactionAsync()
        {
            _transaction =
                await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction is not null)
            {
                await _transaction.CommitAsync();

                await _transaction.DisposeAsync();

                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction is not null)
            {
                await _transaction.RollbackAsync();

                await _transaction.DisposeAsync();

                _transaction = null;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_transaction is not null)
                await _transaction.DisposeAsync();

            await _context.DisposeAsync();
        }
    }
}

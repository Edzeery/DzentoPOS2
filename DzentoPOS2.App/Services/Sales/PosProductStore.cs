using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.Data.Context;
using Microsoft.Extensions.DependencyInjection;

namespace DzentoPOS2.App.Services.Sales;

public interface IPosProductStore
{
    Task<IReadOnlyList<PosProductItem>> GetAllAsync();
    Task<PosProductItem?> FindByVariantIdAsync(Guid variantId);
    Task<IReadOnlyList<PosProductItem>> FindByBarcodeAsync(string barcode);
    void Invalidate();
    void UpdateStock(Guid variantId, decimal delta);
}

/// <summary>
/// In-memory catalog used by the POS screen. The first <see cref="GetAllAsync"/>
/// reads a <see cref="PosProductSnapshot"/> in the background and every later
/// call returns the cached instance immediately.
/// A catalog or branch change invalidates the snapshot. If a load is already
/// running it is allowed to finish, but its result is discarded because the
/// version it started from no longer matches; the same task then reads the new
/// state, so callers never receive a stale snapshot.
/// </summary>
public class PosProductStore : IPosProductStore, IDisposable
{
    private readonly ICatalogChangeNotifier _notifier;
    private readonly IActiveBranchContext _activeBranch;
    private readonly IServiceProvider _serviceProvider;
    private readonly IPosProductSnapshotLoader _loader;

    private readonly object _sync = new();
    private PosProductSnapshot? _snapshot;
    private Task<PosProductSnapshot>? _loadTask;
    private int _version;
    private bool _disposed;

    /// <summary>
    /// The context is accepted for compatibility with existing construction
    /// sites only. Every load runs on a background thread with its own scope,
    /// because a DbContext must not be shared between threads.
    /// </summary>
    public PosProductStore(
        AppDbContext context,
        ICatalogChangeNotifier notifier,
        IActiveBranchContext activeBranch,
        IServiceProvider serviceProvider)
        : this(context, notifier, activeBranch, serviceProvider, PosProductSnapshotLoader.Instance)
    {
    }

    public PosProductStore(
        AppDbContext context,
        ICatalogChangeNotifier notifier,
        IActiveBranchContext activeBranch,
        IServiceProvider serviceProvider,
        IPosProductSnapshotLoader loader)
    {
        _ = context;
        _notifier = notifier;
        _activeBranch = activeBranch;
        _serviceProvider = serviceProvider;
        _loader = loader;

        _notifier.CatalogChanged += OnCatalogChanged;
        _activeBranch.BranchChanged += OnBranchChanged;
    }

    /// <summary>The snapshot currently served, or null before the first load.</summary>
    public PosProductSnapshot? CurrentSnapshot
    {
        get
        {
            lock (_sync)
                return _snapshot;
        }
    }

    /// <summary>Phase breakdown of the last successful load.</summary>
    public PosLoadTimings? LastLoadTimings => CurrentSnapshot?.Timings;

    public async Task<IReadOnlyList<PosProductItem>> GetAllAsync()
    {
        PosProductSnapshot snapshot = await GetSnapshotAsync().ConfigureAwait(false);
        return snapshot.Items;
    }

    public async Task<PosProductItem?> FindByVariantIdAsync(Guid variantId)
    {
        PosProductSnapshot snapshot = await GetSnapshotAsync().ConfigureAwait(false);
        return snapshot.FindByVariantId(variantId);
    }

    public async Task<IReadOnlyList<PosProductItem>> FindByBarcodeAsync(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return Array.Empty<PosProductItem>();

        PosProductSnapshot snapshot = await GetSnapshotAsync().ConfigureAwait(false);
        return snapshot.FindByBarcode(barcode);
    }

    /// <summary>
    /// Drops the cached catalog. A load that is already running sees the new
    /// version and starts over instead of publishing what it just read.
    /// </summary>
    public void Invalidate()
    {
        lock (_sync)
            InvalidateLocked();
    }

    public void UpdateStock(Guid variantId, decimal delta) =>
        _notifier.NotifyStockChanged(variantId, delta);

    private Task<PosProductSnapshot> GetSnapshotAsync()
    {
        lock (_sync)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_snapshot is not null)
                return Task.FromResult(_snapshot);

            // Single flight: every caller waits on the same load.
            return _loadTask ??= Task.Run(LoadLoopAsync);
        }
    }

    private async Task<PosProductSnapshot> LoadLoopAsync()
    {
        while (true)
        {
            int version;
            Guid branchId;
            lock (_sync)
            {
                version = _version;
                branchId = _activeBranch.ActiveBranchId;
            }

            PosProductSnapshot snapshot;
            try
            {
                // A load owns a fresh scope; the store's caller keeps its own
                // DbContext and the two never touch the same instance.
                using IServiceScope scope = _serviceProvider.CreateScope();
                AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                snapshot = await _loader
                    .LoadAsync(context, branchId, CancellationToken.None)
                    .ConfigureAwait(false);
            }
            catch
            {
                lock (_sync)
                {
                    // Let the next caller retry instead of handing out a
                    // faulted task. A load that fails because its owner was
                    // disposed completes with an empty catalog.
                    _loadTask = null;

                    if (_disposed)
                        return PosProductSnapshot.Empty;
                }

                throw;
            }

            lock (_sync)
            {
                if (_disposed)
                    return snapshot;

                if (version == _version && branchId == _activeBranch.ActiveBranchId)
                {
                    _snapshot = snapshot;
                    return snapshot;
                }

                // The catalog or the branch changed while the rows were read.
                // The snapshot is stale: drop it and read again.
            }
        }
    }

    private void OnCatalogChanged(object? sender, CatalogChangedEventArgs e)
    {
        lock (_sync)
        {
            if (_disposed) return;

            if (e.FullInvalidation)
            {
                InvalidateLocked();
                return;
            }

            // A stock delta is optimistic: the sale already wrote it, so the
            // cached row is patched instead of reloading the whole catalog.
            if (e.VariantId.HasValue && e.StockDelta != 0)
                _snapshot?.ApplyStockDelta(e.VariantId.Value, e.StockDelta);
        }
    }

    private void OnBranchChanged(object? sender, ActiveBranchChangedEventArgs e)
    {
        // Stock depends on the warehouse of the active branch, so a branch
        // change invalidates everything.
        lock (_sync)
        {
            if (_disposed) return;

            InvalidateLocked();
        }
    }

    private void InvalidateLocked()
    {
        _version++;
        _snapshot = null;

        // An in-flight load notices the version change and reads again, so it
        // is kept. A finished task is dropped to make the next caller start one.
        if (_loadTask is null || _loadTask.IsCompleted)
            _loadTask = null;
    }

    public void Dispose()
    {
        lock (_sync)
        {
            if (_disposed) return;

            _disposed = true;
            _snapshot = null;
            _loadTask = null;
        }

        _notifier.CatalogChanged -= OnCatalogChanged;
        _activeBranch.BranchChanged -= OnBranchChanged;
    }
}

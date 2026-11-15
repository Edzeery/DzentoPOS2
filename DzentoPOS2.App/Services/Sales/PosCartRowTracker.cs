using System;
using System.Collections.Generic;
using DzentoPOS2.Shared.Contracts.DTOs.Sales;

namespace DzentoPOS2.App.Services.Sales;

/// <summary>Identity of a cart row: the same fields the cart merger uses.</summary>
public readonly record struct PosCartRowKey(
    Guid ProductId,
    Guid VariantId,
    Guid BatchId,
    Guid SerialUnitId)
{
    public static PosCartRowKey From(CreateSaleItemDto line)
    {
        ArgumentNullException.ThrowIfNull(line);
        return new PosCartRowKey(line.ProductId, line.VariantId, line.BatchId, line.SerialUnitId);
    }
}

/// <summary>The values a cart row displays, captured when the row was applied.</summary>
public readonly record struct PosCartRowSnapshot(
    PosCartRowKey Key,
    decimal Quantity,
    decimal Price);

/// <summary>
/// What changed between two cart states. Indices refer to the positions
/// described in each property's documentation.
/// </summary>
public sealed class PosCartRowDiff
{
    public static PosCartRowDiff NoChanges { get; } = new(
        fullReset: false,
        addedRows: Array.Empty<int>(),
        removedRows: Array.Empty<int>(),
        changedRows: Array.Empty<int>(),
        orderChanged: false);

    public static PosCartRowDiff FullRebuild { get; } = new(
        fullReset: true,
        addedRows: Array.Empty<int>(),
        removedRows: Array.Empty<int>(),
        changedRows: Array.Empty<int>(),
        orderChanged: false);

    public PosCartRowDiff(
        bool fullReset,
        IReadOnlyList<int> addedRows,
        IReadOnlyList<int> removedRows,
        IReadOnlyList<int> changedRows,
        bool orderChanged)
    {
        FullReset = fullReset;
        AddedRows = addedRows ?? Array.Empty<int>();
        RemovedRows = removedRows ?? Array.Empty<int>();
        ChangedRows = changedRows ?? Array.Empty<int>();
        OrderChanged = orderChanged;
    }

    /// <summary>The caller must rebuild every row and ignore the index lists.</summary>
    public bool FullReset { get; }

    /// <summary>Indices in the new cart that need a new row, ascending.</summary>
    public IReadOnlyList<int> AddedRows { get; }

    /// <summary>Indices in the previous row list to delete, ascending.</summary>
    public IReadOnlyList<int> RemovedRows { get; }

    /// <summary>Indices in the new cart whose quantity or price changed.</summary>
    public IReadOnlyList<int> ChangedRows { get; }

    /// <summary>Surviving rows are in a different relative order.</summary>
    public bool OrderChanged { get; }

    public bool HasChanges =>
        FullReset || AddedRows.Count > 0 || RemovedRows.Count > 0
        || ChangedRows.Count > 0 || OrderChanged;
}

/// <summary>
/// Turns successive cart snapshots into row operations. The cart screen uses
/// it to add, remove and edit single grid rows instead of clearing and
/// rebinding the whole grid on every change.
/// The tracker is not thread-safe and belongs to the UI thread that owns the
/// cart.
/// </summary>
public sealed class PosCartRowTracker
{
    private List<PosCartRowSnapshot> _rows = new();

    public int Count => _rows.Count;

    public IReadOnlyList<PosCartRowSnapshot> Rows => _rows;

    /// <summary>Forgets every known row, e.g. after the grid was rebuilt.</summary>
    public void Clear() => _rows = new List<PosCartRowSnapshot>();

    /// <summary>
    /// Compares the cart with the previous state and returns the row
    /// operations that bring the grid up to date.
    /// </summary>
    public PosCartRowDiff Apply(IReadOnlyList<CreateSaleItemDto> cart)
    {
        ArgumentNullException.ThrowIfNull(cart);

        var next = new List<PosCartRowSnapshot>(cart.Count);
        var nextKeys = new HashSet<PosCartRowKey>();
        bool duplicateKeys = false;
        foreach (CreateSaleItemDto line in cart)
        {
            ArgumentNullException.ThrowIfNull(line);
            var snapshot = new PosCartRowSnapshot(PosCartRowKey.From(line), line.Quantity, line.Price);

            // The merger guarantees one line per key. A duplicate means the
            // caller bypassed it, so an incremental update is unsafe.
            if (!nextKeys.Add(snapshot.Key))
                duplicateKeys = true;

            next.Add(snapshot);
        }

        if (duplicateKeys)
        {
            Commit(next);
            return PosCartRowDiff.FullRebuild;
        }

        // First fill: give the caller every index and the reset flag.
        if (_rows.Count == 0)
        {
            Commit(next);
            if (next.Count == 0)
                return PosCartRowDiff.NoChanges;

            var all = new int[next.Count];
            for (int i = 0; i < all.Length; i++) all[i] = i;
            return new PosCartRowDiff(true, all, Array.Empty<int>(), Array.Empty<int>(), false);
        }

        // Everything removed.
        if (next.Count == 0)
        {
            var removedAll = new int[_rows.Count];
            for (int i = 0; i < removedAll.Length; i++) removedAll[i] = i;
            Commit(next);
            return new PosCartRowDiff(false, Array.Empty<int>(), removedAll, Array.Empty<int>(), false);
        }

        var previousKeys = new HashSet<PosCartRowKey>(_rows.Count);
        var previousByKey = new Dictionary<PosCartRowKey, PosCartRowSnapshot>(_rows.Count);
        foreach (PosCartRowSnapshot row in _rows)
        {
            previousKeys.Add(row.Key);
            previousByKey[row.Key] = row;
        }

        var removed = new List<int>();
        for (int i = 0; i < _rows.Count; i++)
            if (!nextKeys.Contains(_rows[i].Key))
                removed.Add(i);

        var added = new List<int>();
        var changed = new List<int>();
        for (int i = 0; i < next.Count; i++)
        {
            PosCartRowSnapshot row = next[i];
            if (!previousByKey.TryGetValue(row.Key, out PosCartRowSnapshot previous))
            {
                added.Add(i);
                continue;
            }

            if (previous.Quantity != row.Quantity || previous.Price != row.Price)
                changed.Add(i);
        }

        bool orderChanged = !SurvivingOrderMatches(next, previousKeys, nextKeys);
        Commit(next);

        if (!orderChanged && added.Count == 0 && removed.Count == 0 && changed.Count == 0)
            return PosCartRowDiff.NoChanges;

        return new PosCartRowDiff(false, added, removed, changed, orderChanged);
    }

    private void Commit(List<PosCartRowSnapshot> next) => _rows = next;

    /// <summary>
    /// Removed rows do not disturb the order of the remaining ones; a real
    /// reorder does.
    /// </summary>
    private bool SurvivingOrderMatches(
        List<PosCartRowSnapshot> next,
        HashSet<PosCartRowKey> previousKeys,
        HashSet<PosCartRowKey> nextKeys)
    {
        int previousIndex = 0;
        foreach (PosCartRowSnapshot nextRow in next)
        {
            if (!previousKeys.Contains(nextRow.Key))
                continue;

            while (previousIndex < _rows.Count
                   && !nextKeys.Contains(_rows[previousIndex].Key))
            {
                previousIndex++;
            }

            if (previousIndex >= _rows.Count || _rows[previousIndex].Key != nextRow.Key)
                return false;

            previousIndex++;
        }

        return true;
    }
}

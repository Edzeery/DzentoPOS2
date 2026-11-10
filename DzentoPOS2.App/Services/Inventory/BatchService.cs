using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Core.Enums;
using DzentoPOS2.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.App.Services.Inventory;

public interface IBatchService
{
    Task<Batch?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Batch>> GetByVariantAsync(Guid variantId, CancellationToken ct = default);
    Task<List<Batch>> GetByWarehouseAsync(Guid warehouseId, CancellationToken ct = default);
    Task<List<Batch>> GetExpiringWithinDaysAsync(int days, CancellationToken ct = default);
    Task<Batch> CreateAsync(Guid variantId, Guid warehouseId, string batchNumber, DateTime? expiryDate, decimal quantity, decimal unitCost, CancellationToken ct = default);
    Task AdjustQuantityAsync(Guid batchId, decimal quantityChange, CancellationToken ct = default);
    Task<List<Batch>> GetFEFOSelectionAsync(Guid variantId, Guid warehouseId, decimal requiredQuantity, CancellationToken ct = default);
}

public class BatchService : IBatchService
{
    private readonly AppDbContext _context;

    public BatchService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Batch?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Batches
            .AsNoTracking()
            .Include(b => b.Variant)
            .Include(b => b.Warehouse)
            .FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public async Task<List<Batch>> GetByVariantAsync(Guid variantId, CancellationToken ct = default)
    {
        return await _context.Batches
            .AsNoTracking()
            .Where(b => b.VariantId == variantId)
            .OrderBy(b => b.ExpiryDate)
            .ThenBy(b => b.ReceivedAt)
            .ToListAsync(ct);
    }

    public async Task<List<Batch>> GetByWarehouseAsync(Guid warehouseId, CancellationToken ct = default)
    {
        return await _context.Batches
            .AsNoTracking()
            .Where(b => b.WarehouseId == warehouseId)
            .OrderBy(b => b.ExpiryDate)
            .ThenBy(b => b.ReceivedAt)
            .ToListAsync(ct);
    }

    public async Task<List<Batch>> GetExpiringWithinDaysAsync(int days, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(days);
        return await _context.Batches
            .AsNoTracking()
            .Where(b => b.ExpiryDate.HasValue && b.ExpiryDate.Value <= cutoff && b.ExpiryDate.Value >= DateTime.UtcNow)
            .Include(b => b.Variant)
            .Include(b => b.Warehouse)
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync(ct);
    }

    public async Task<Batch> CreateAsync(Guid variantId, Guid warehouseId, string batchNumber, DateTime? expiryDate, decimal quantity, decimal unitCost, CancellationToken ct = default)
    {
        var batch = new Batch
        {
            VariantId = variantId,
            WarehouseId = warehouseId,
            BatchNumber = batchNumber,
            ExpiryDate = expiryDate,
            Quantity = quantity,
            UnitCost = unitCost,
            ReceivedAt = DateTime.UtcNow
        };

        _context.Batches.Add(batch);
        await _context.SaveChangesAsync(ct);
        return batch;
    }

    public async Task AdjustQuantityAsync(Guid batchId, decimal quantityChange, CancellationToken ct = default)
    {
        var batch = await _context.Batches.FirstOrDefaultAsync(b => b.Id == batchId, ct);
        if (batch == null) return;

        batch.Quantity += quantityChange;
        if (batch.Quantity < 0) batch.Quantity = 0;

        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<Batch>> GetFEFOSelectionAsync(Guid variantId, Guid warehouseId, decimal requiredQuantity, CancellationToken ct = default)
    {
        var batches = await _context.Batches
            .AsNoTracking()
            .Where(b => b.VariantId == variantId && b.WarehouseId == warehouseId && b.Quantity > 0)
            .OrderBy(b => b.ExpiryDate ?? DateTime.MaxValue)
            .ThenBy(b => b.ReceivedAt)
            .ToListAsync(ct);

        var selected = new List<Batch>();
        decimal remaining = requiredQuantity;

        foreach (var batch in batches)
        {
            if (remaining <= 0) break;
            var take = Math.Min(remaining, batch.Quantity);
            selected.Add(new Batch
            {
                Id = batch.Id,
                VariantId = batch.VariantId,
                WarehouseId = batch.WarehouseId,
                BatchNumber = batch.BatchNumber,
                ExpiryDate = batch.ExpiryDate,
                Quantity = take,
                UnitCost = batch.UnitCost,
                ReceivedAt = batch.ReceivedAt
            });
            remaining -= take;
        }

        return selected;
    }
}

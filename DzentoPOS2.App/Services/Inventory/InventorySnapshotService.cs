using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Shared.Contracts.DTOs.Inventory;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DzentoPOS2.App.Services.Inventory
{
    public interface IInventorySnapshotService
    {
        Task UpsertTodayAsync(InventoryValuationDto valuation, CancellationToken cancellationToken = default);
    }

    public sealed class InventorySnapshotService : IInventorySnapshotService
    {
        private readonly AppDbContext _context;
        private readonly IBusinessClock _clock;
        private readonly IActiveBranchContext _activeBranch;

        public InventorySnapshotService(AppDbContext context, IBusinessClock clock, IActiveBranchContext activeBranch)
        {
            _context = context;
            _clock = clock;
            _activeBranch = activeBranch;
        }

        public async Task UpsertTodayAsync(InventoryValuationDto valuation, CancellationToken cancellationToken = default)
        {
            // Only write for the active branch (never for "all branches" scope).
            var branchId = _activeBranch.ActiveBranchId;

            var localToday = _clock.Now.Date;

            var existing = await _context.InventorySnapshots
                .FirstOrDefaultAsync(s => s.BranchId == branchId && s.SnapshotDate == localToday, cancellationToken);

            if (existing is null)
            {
                existing = new InventorySnapshot { BranchId = branchId, SnapshotDate = localToday };
                _context.InventorySnapshots.Add(existing);
            }

            existing.TotalQuantity = valuation.TotalQuantity;
            existing.CostValue = valuation.CostValue;
            existing.RetailValue = valuation.RetailValue;
            existing.ExpectedProfit = valuation.ExpectedProfit;
            existing.AtRiskProfit = valuation.AtRiskProfit;
            existing.DiscountFactorAvg = valuation.DiscountFactorAvg;
            existing.FeeFactor = valuation.FeeFactor;

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
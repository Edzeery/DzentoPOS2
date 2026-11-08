using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.App.Authorization;
using DzentoPOS2.App.Services.Purchases;
using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.Core.Constants;
using DzentoPOS2.App.Session;
using DzentoPOS2.Core.Entities.Purchases;
using DzentoPOS2.Core.Enums;
using DzentoPOS2.Core.ValueObjects.Common;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common.Interfaces;
using DzentoPOS2.Data.Repositories.Purchases.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Purchases;
using DzentoPOS2.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.App.Services.Purchases
{
    public class PurchaseService : IPurchaseService
    {
        private readonly IPurchaseInvoiceRepository _invoiceRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppDbContext _context;
        private readonly IBusinessClock _clock;
        private readonly IStoreContextService _storeContext;
        private readonly IDocumentNumberService _documentNumbers;
        private readonly IStockService _stockService;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuthorizationGuard _guard;
    private readonly RegionalContext _regional;
        private readonly IInventoryChangedNotifier _inventoryNotifier;

        public PurchaseService(
            IPurchaseInvoiceRepository invoiceRepo,
            IUnitOfWork unitOfWork,
            AppDbContext context,
            IBusinessClock clock,
            IStoreContextService storeContext,
            IDocumentNumberService documentNumbers,
            IStockService stockService,
            ICurrentUserService currentUser,
            IAuthorizationGuard guard,
            RegionalContext regional,
            IInventoryChangedNotifier inventoryNotifier)
        {
            _invoiceRepo = invoiceRepo;
            _unitOfWork = unitOfWork;
            _context = context;
            _clock = clock;
            _storeContext = storeContext;
            _documentNumbers = documentNumbers;
            _stockService = stockService;
            _currentUser = currentUser;
            _guard = guard;
            _regional = regional;
            _inventoryNotifier = inventoryNotifier;
        }

        public async Task<List<PurchaseInvoiceDto>> GetAllAsync()
        {
            var invoices = await _context.PurchaseInvoices
                .AsNoTracking()
                .Include(x => x.Items)
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return invoices.Select(Map).ToList();
        }

        public async Task<PurchaseInvoiceDto?> GetByIdAsync(Guid id)
        {
            var invoice = await _invoiceRepo.GetWithItemsAsync(id);
            if (invoice == null) return null;

            var supplierName = await _context.Suppliers
                .AsNoTracking()
                .Where(s => s.Id == invoice.SupplierId && !s.IsDeleted)
                .Select(s => s.Name)
                .FirstOrDefaultAsync() ?? "";

            var dto = Map(invoice);
            dto.SupplierName = supplierName;
            return dto;
        }

        public async Task<List<PurchaseInvoiceDto>> GetBySupplierAsync(Guid supplierId)
        {
            var invoices = await _context.PurchaseInvoices
                .AsNoTracking()
                .Include(x => x.Items)
                .Where(x => x.SupplierId == supplierId && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return invoices.Select(Map).ToList();
        }

        public async Task<PurchaseInvoiceDto> CreateAsync(CreatePurchaseInvoiceDto dto)
        {
            await _guard.DemandAsync(Permissions.PurchasesCreate);

            if (dto.Items.Count == 0)
                throw new InvalidOperationException("Purchase must have at least one item");

            var branchId = await _storeContext.GetDefaultBranchIdAsync();
            var warehouseId = await _storeContext.GetDefaultWarehouseIdAsync(branchId);
            var total = dto.Items.Sum(i => i.Quantity * i.CostPrice);
            var remaining = total - dto.PaidAmount;
            if (remaining < 0) remaining = 0;

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var invoiceNumber = await _documentNumbers
                    .NextDocumentNumberAsync("PUR");

                var invoice = new PurchaseInvoice
                {
                    InvoiceNumber = invoiceNumber,
                    SupplierId = dto.SupplierId,
                    BranchId = branchId,
                    Total = total,
                    PaidAmount = dto.PaidAmount,
                    RemainingAmount = remaining
                };

                var purchaseProductIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
                var purchaseDefaultVariants = await _context.ProductVariants
                    .AsNoTracking()
                    .Where(v => purchaseProductIds.Contains(v.ProductId) && v.IsDefault)
                    .ToDictionaryAsync(v => v.ProductId, v => v.Id);

                foreach (var item in dto.Items)
                {
                    if (!purchaseDefaultVariants.TryGetValue(item.ProductId, out var pVariantId))
                    {
                        var prod = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == item.ProductId);
                        var v = new Core.Entities.Inventory.ProductVariant
                        {
                            ProductId = item.ProductId,
                            Name = prod?.Name ?? string.Empty,
                            CostPrice = prod?.CostPrice ?? new Core.ValueObjects.Common.Money(item.CostPrice, _regional.CurrencyCode),
                            SalePrice = prod?.SalePrice ?? new Core.ValueObjects.Common.Money(item.CostPrice, _regional.CurrencyCode),
                            MinimumStock = prod?.MinimumStock ?? new Core.ValueObjects.Inventory.Quantity(0),
                            TrackStock = prod?.TrackStock ?? true,
                            IsActive = prod?.IsActive ?? true,
                            IsDefault = true
                        };
                        await _context.ProductVariants.AddAsync(v);
                        await _context.SaveChangesAsync();
                        pVariantId = v.Id;
                        purchaseDefaultVariants[item.ProductId] = pVariantId;
                    }
                    invoice.Items.Add(new PurchaseInvoiceItem
                    {
                        ProductId = item.ProductId,
                        VariantId = pVariantId,
                        Quantity = item.Quantity,
                        CostPrice = item.CostPrice,
                        Total = item.Quantity * item.CostPrice
                    });
                }

                // Moving weighted average cost update (tracked products only).
                // Cost is tracked per variant now; the default variant stays in
                // sync with the legacy Product.CostPrice used by reports.
                var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
                var products = await _context.Products
                    .Where(p => productIds.Contains(p.Id) && !p.IsDeleted)
                    .ToDictionaryAsync(p => p.Id);

                var variantIds = purchaseDefaultVariants.Values.Distinct().ToList();
                var stockTotals = (await _context.ProductStocks
                    .Where(s => variantIds.Contains(s.VariantId))
                    .GroupBy(s => s.VariantId)
                    .Select(g => new { VariantId = g.Key, Qty = g.Sum(x => x.Quantity.Value) })
                    .ToListAsync())
                    .ToDictionary(x => x.VariantId, x => x.Qty);

                var variants = await _context.ProductVariants
                    .Where(v => variantIds.Contains(v.Id))
                    .ToDictionaryAsync(v => v.Id);

                foreach (var group in dto.Items.GroupBy(i => i.ProductId))
                {
                    if (!products.TryGetValue(group.Key, out var product) || !product.TrackStock)
                        continue;

                    if (!purchaseDefaultVariants.TryGetValue(group.Key, out var variantId))
                        continue;

                    if (!variants.TryGetValue(variantId, out var variant))
                        continue;

                    var oldQty = stockTotals.TryGetValue(variantId, out var q) ? q : 0m;
                    var inQty = group.Sum(i => i.Quantity);
                    if (inQty <= 0) continue;
                    var inCostTotal = group.Sum(i => i.Quantity * i.CostPrice);
                    var inCostAvg = inCostTotal / inQty;

                    decimal newAvg;
                    if (oldQty <= 0)
                    {
                        newAvg = inCostAvg;
                    }
                    else
                    {
                        var oldCost = variant.CostPrice?.Amount ?? 0m;
                        newAvg = (oldQty * oldCost + inQty * inCostAvg) / (oldQty + inQty);
                    }

                    var rounded = decimal.Round(
                        newAvg,
                        _regional.CurrencyDecimalDigits,
                        MidpointRounding.AwayFromZero);

                    variant.CostPrice = new Money(rounded, _regional.CurrencyCode);

                    if (variant.IsDefault)
                        product.CostPrice = new Money(rounded, _regional.CurrencyCode);
                }

                await _context.PurchaseInvoices.AddAsync(invoice);

                if (dto.PaidAmount > 0)
                {
                    await _context.PurchasePayments.AddAsync(new PurchasePayment
                    {
                        PurchaseInvoiceId = invoice.Id,
                        Amount = dto.PaidAmount,
                        PaymentMethod = dto.PaymentMethod,
                        PaymentDate = _clock.UtcNow
                    });
                }

                await _stockService.ApplyPurchaseAsync(
                    invoice.Id,
                    invoice.InvoiceNumber,
                    warehouseId,
                    dto.Items
                        .Select(i => new StockChange(i.ProductId, purchaseDefaultVariants[i.ProductId], i.Quantity))
                        .ToList(),
                    _currentUser.UserId);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _inventoryNotifier.NotifyChanged();
                return Map(invoice);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                _context.ChangeTracker.Clear();
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            await _guard.DemandAsync(Permissions.PurchasesDelete);

            var invoice = await _invoiceRepo.GetWithItemsAsync(id);
            if (invoice == null) return;

            var warehouseId = await _storeContext
                .GetDefaultWarehouseIdAsync(invoice.BranchId);

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                await _stockService.ReverseAsync(
                    invoice.Id,
                    "PurchaseInvoiceVoid",
                    $"VOID-{invoice.InvoiceNumber}",
                    warehouseId,
                    invoice.Items
                        .Select(i => new StockChange(i.ProductId, i.VariantId, i.Quantity))
                        .ToList(),
                    MovementType.PurchaseReturn,
                    _currentUser.UserId);

                _invoiceRepo.Remove(invoice);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _inventoryNotifier.NotifyChanged();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                _context.ChangeTracker.Clear();
                throw;
            }
        }

        public async Task<decimal> GetTodayTotalAsync()
        {
            var (todayStart, tomorrowStart) = _clock.TodayRangeUtc();

            return await _context.PurchaseInvoices
                .AsNoTracking()
                .Where(x => !x.IsDeleted &&
                    x.CreatedAt >= todayStart &&
                    x.CreatedAt < tomorrowStart)
                .SumAsync(x => x.Total);
        }

        public async Task<int> GetTodayCountAsync()
        {
            var (todayStart, tomorrowStart) = _clock.TodayRangeUtc();

            return await _context.PurchaseInvoices
                .AsNoTracking()
                .CountAsync(x => !x.IsDeleted &&
                    x.CreatedAt >= todayStart &&
                    x.CreatedAt < tomorrowStart);
        }

        private static PurchaseInvoiceDto Map(PurchaseInvoice invoice)
        {
            return new PurchaseInvoiceDto
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                SupplierId = invoice.SupplierId,
                Total = invoice.Total,
                PaidAmount = invoice.PaidAmount,
                RemainingAmount = invoice.RemainingAmount,
                CreatedAt = invoice.CreatedAt,
                Items = invoice.Items?.Select(i => new PurchaseItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    CostPrice = i.CostPrice,
                    Total = i.Total
                }).ToList() ?? new()
            };
        }
    }
}

using DzentoPOS2.App.Services.Inventory;
using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.App.Session;
using DzentoPOS2.Core.Entities.Sales;
using DzentoPOS2.Core.Enums;
using DzentoPOS2.Core.Services.Sales;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common.Interfaces;
using DzentoPOS2.Data.Repositories.Sales.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Sales;
using DzentoPOS2.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.App.Services.Sales
{
    public class SaleService : ISaleService
    {
        private readonly ISalesInvoiceRepository _invoiceRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppDbContext _context;
        private readonly IBusinessClock _clock;
        private readonly IStoreContextService _storeContext;
        private readonly IDocumentNumberService _documentNumbers;
        private readonly IStockService _stockService;
        private readonly ICurrentUserService _currentUser;
        private readonly RegionalContext _regional;

        public SaleService(
            ISalesInvoiceRepository invoiceRepo,
            IUnitOfWork unitOfWork,
            AppDbContext context,
            IBusinessClock clock,
            IStoreContextService storeContext,
            IDocumentNumberService documentNumbers,
            IStockService stockService,
            ICurrentUserService currentUser,
            RegionalContext regional)
        {
            _invoiceRepo = invoiceRepo;
            _unitOfWork = unitOfWork;
            _context = context;
            _clock = clock;
            _storeContext = storeContext;
            _documentNumbers = documentNumbers;
            _stockService = stockService;
            _currentUser = currentUser;
            _regional = regional;
        }

        public async Task<List<SaleDto>> GetAllAsync()
        {
            var invoices = await _context.SalesInvoices
                .AsNoTracking()
                .Include(x => x.Items)
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return invoices.Select(Map).ToList();
        }

        public async Task<SaleDto?> GetByIdAsync(Guid id)
        {
            var invoice = await _invoiceRepo.GetWithItemsAsync(id);
            if (invoice == null) return null;

            var customerName = await _context.Customers
                .AsNoTracking()
                .Where(c => c.Id == invoice.CustomerId && !c.IsDeleted)
                .Select(c => c.FullName)
                .FirstOrDefaultAsync() ?? "";

            var dto = Map(invoice);
            dto.CustomerName = customerName;
            return dto;
        }

        public async Task<List<SaleDto>> GetByCustomerAsync(Guid customerId)
        {
            var invoices = await _context.SalesInvoices
                .AsNoTracking()
                .Include(x => x.Items)
                .Where(x => x.CustomerId == customerId && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return invoices.Select(Map).ToList();
        }

        public async Task<SaleDto> CreateAsync(CreateSaleDto dto)
        {
            if (dto.Items.Count == 0)
                throw new InvalidOperationException("Sale must have at least one item");

            var lines = dto.Items
                .Select(i => new InvoiceLine(i.Quantity, i.Price))
                .ToList();

            var branchId = await _storeContext.GetDefaultBranchIdAsync();
            var warehouseId = await _storeContext.GetDefaultWarehouseIdAsync(branchId);
            var cashboxId = await _storeContext.GetDefaultCashboxIdAsync(branchId);
            var currency = _regional.CurrencyCode;
            var digits = _regional.CurrencyDecimalDigits;

            var openSession = await _context.CashSessions
                .FirstOrDefaultAsync(x =>
                    x.CashboxId == cashboxId &&
                    x.Status == Core.Enums.CashSessionStatus.Open &&
                    !x.IsDeleted)
                ?? throw new DzentoPOS2.Core.Exceptions.CashSessionRequiredException();

            if (openSession.OpeningBalance < 0)
                throw new DzentoPOS2.Core.Exceptions.CashSessionRequiredException();

            foreach (var item in dto.Items)
            {
                var product = await _context.Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId && !p.IsDeleted);
                if (product == null || !product.IsActive)
                    throw new InvalidOperationException("Invalid or inactive product");
            }

            for (var attempt = 0; ; attempt++)
            {
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    var invoiceNumber =
                        await _documentNumbers.NextInvoiceNumberAsync();

                    var invoice = SalesInvoice.Create(
                        invoiceNumber,
                        dto.CustomerId,
                        branchId,
                        lines,
                        dto.Discount,
                        dto.TaxRate,
                        dto.PaidAmount,
                        currency,
                        digits);

                    foreach (var item in dto.Items)
                    {
                        var line = new SalesInvoiceItem
                        {
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            Price = item.Price,
                            Total = Math.Round(item.Quantity * item.Price, digits, MidpointRounding.AwayFromZero),
                            Description = string.IsNullOrWhiteSpace(item.Description) ? string.Empty : item.Description.Trim()
                        };
                        invoice.Items.Add(line);
                    }

                invoice.AssignCashSession(openSession.Id);
                await _context.SalesInvoices.AddAsync(invoice);

                if (dto.PaidAmount > 0)
                {
                    var paymentMethod = string.IsNullOrWhiteSpace(dto.PaymentMethod) ? "Cash" : dto.PaymentMethod;
                    await _context.SalesPayments.AddAsync(new SalesPayment
                    {
                        SalesInvoiceId = invoice.Id,
                        Amount = dto.PaidAmount,
                        PaymentMethod = paymentMethod,
                        PaymentDate = _clock.UtcNow
                    });

                    if (string.Equals(paymentMethod, "Cash", StringComparison.OrdinalIgnoreCase))
                    {
                        await _context.CashTransactions.AddAsync(new Core.Entities.Accounting.CashTransaction
                        {
                            CashboxId = cashboxId,
                            CashSessionId = openSession.Id,
                            Amount = dto.PaidAmount,
                            TransactionType = Core.Enums.MovementType.SaleCashIn,
                            SourceType = "SalesInvoice",
                            SourceId = invoice.Id,
                            ReferenceNumber = invoice.InvoiceNumber.Value,
                            Notes = "Cash sale",
                            TransactionDate = _clock.UtcNow
                        });
                    }
                }

                    await _stockService.ApplySaleAsync(
                        invoice.Id,
                        invoice.InvoiceNumber.Value,
                        warehouseId,
                        dto.Items
                            .Select(i => new StockChange(i.ProductId, i.Quantity))
                            .ToList(),
                        _currentUser.UserId);

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    return Map(invoice);
                }
                catch (DbUpdateConcurrencyException) when (attempt == 0)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _context.ChangeTracker.Clear();
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _context.ChangeTracker.Clear();
                    throw;
                }
            }
        }

        public Task DeleteAsync(Guid id)
        {
            return VoidAsync(id, "Voided by user");
        }

        public async Task VoidAsync(Guid id, string reason)
        {
            var invoice = await _invoiceRepo.GetWithItemsAsync(id);
            if (invoice == null || invoice.IsVoided)
                return;

            if (string.IsNullOrWhiteSpace(reason))
                throw new InvalidOperationException("A void reason is required");

            var warehouseId = await _storeContext
                .GetDefaultWarehouseIdAsync(invoice.BranchId);

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var cashboxId = await _storeContext
                    .GetDefaultCashboxIdAsync(invoice.BranchId);

                await _stockService.ReverseAsync(
                    invoice.Id,
                    "SalesInvoiceVoid",
                    $"VOID-{invoice.InvoiceNumber.Value}",
                    warehouseId,
                    invoice.Items
                        .Select(i => new StockChange(i.ProductId, i.Quantity))
                        .ToList(),
                    MovementType.SaleVoidIn,
                    _currentUser.UserId);

                invoice.Void(_currentUser.UserId, reason, _clock.UtcNow);

                var cashPaid = invoice.PaidAmount?.Amount ?? 0;
                if (cashPaid > 0 && invoice.CashSessionId.HasValue)
                {
                    await _context.CashTransactions.AddAsync(new Core.Entities.Accounting.CashTransaction
                    {
                        CashboxId = cashboxId,
                        CashSessionId = invoice.CashSessionId.Value,
                        Amount = cashPaid,
                        TransactionType = Core.Enums.MovementType.SaleVoidCashOut,
                        SourceType = "SalesInvoiceVoid",
                        SourceId = invoice.Id,
                        ReferenceNumber = $"VOID-{invoice.InvoiceNumber.Value}",
                        Notes = reason.Trim(),
                        TransactionDate = _clock.UtcNow
                    });
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
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

            return await _context.SalesInvoices
                .AsNoTracking()
                .Where(x => !x.IsDeleted &&
                    x.CreatedAt >= todayStart &&
                    x.CreatedAt < tomorrowStart)
                .SumAsync(x => x.Total.Amount);
        }

        public async Task<int> GetTodayCountAsync()
        {
            var (todayStart, tomorrowStart) = _clock.TodayRangeUtc();

            return await _context.SalesInvoices
                .AsNoTracking()
                .CountAsync(x => !x.IsDeleted &&
                    x.CreatedAt >= todayStart &&
                    x.CreatedAt < tomorrowStart);
        }

        private static SaleDto Map(SalesInvoice invoice)
        {
            return new SaleDto
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber?.Value ?? "",
                CustomerId = invoice.CustomerId,
                Subtotal = invoice.Subtotal?.Amount ?? 0,
                Discount = invoice.Discount?.Value ?? 0,
                DiscountAmount = invoice.DiscountAmount?.Amount ?? 0,
                TaxRate = invoice.Tax?.Rate ?? 0,
                TaxAmount = invoice.TaxAmount?.Amount ?? 0,
                Total = invoice.Total?.Amount ?? 0,
                PaidAmount = invoice.PaidAmount?.Amount ?? 0,
                RemainingAmount = invoice.RemainingAmount?.Amount ?? 0,
                IsVoided = invoice.IsVoided,
                VoidReason = invoice.VoidReason,
                VoidedAtUtc = invoice.VoidedAtUtc,
                CreatedAt = invoice.CreatedAt,
                Items = invoice.Items?.Select(i => new SaleItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Price,
                    Total = i.Total,
                    Description = i.Description ?? string.Empty
                }).ToList() ?? new()
            };
        }
    }
}

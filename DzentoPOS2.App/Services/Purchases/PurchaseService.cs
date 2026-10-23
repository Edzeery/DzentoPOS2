using DzentoPOS2.App.Services.Purchases;
using DzentoPOS2.Core.Entities.Purchases;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common.Interfaces;
using DzentoPOS2.Data.Repositories.Purchases.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Purchases;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.App.Services.Purchases
{
    public class PurchaseService : IPurchaseService
    {
        private readonly IPurchaseInvoiceRepository _invoiceRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppDbContext _context;

        public PurchaseService(
            IPurchaseInvoiceRepository invoiceRepo,
            IUnitOfWork unitOfWork,
            AppDbContext context)
        {
            _invoiceRepo = invoiceRepo;
            _unitOfWork = unitOfWork;
            _context = context;
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
            if (dto.Items.Count == 0)
                throw new InvalidOperationException("Purchase must have at least one item");

            var invoiceNumber = await GenerateInvoiceNumberAsync();
            var total = dto.Items.Sum(i => i.Quantity * i.CostPrice);
            var remaining = total - dto.PaidAmount;
            if (remaining < 0) remaining = 0;

            var invoice = new PurchaseInvoice
            {
                InvoiceNumber = invoiceNumber,
                SupplierId = dto.SupplierId,
                BranchId = Guid.Empty,
                Total = total,
                PaidAmount = dto.PaidAmount,
                RemainingAmount = remaining
            };

            foreach (var item in dto.Items)
            {
                invoice.Items.Add(new PurchaseInvoiceItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    CostPrice = item.CostPrice,
                    Total = item.Quantity * item.CostPrice
                });
            }

            await _context.PurchaseInvoices.AddAsync(invoice);

            if (dto.PaidAmount > 0)
            {
                await _context.PurchasePayments.AddAsync(new PurchasePayment
                {
                    PurchaseInvoiceId = invoice.Id,
                    Amount = dto.PaidAmount,
                    PaymentMethod = dto.PaymentMethod,
                    PaymentDate = DateTime.UtcNow
                });
            }

            await _unitOfWork.SaveChangesAsync();

            return Map(invoice);
        }

        public async Task DeleteAsync(Guid id)
        {
            var invoice = await _invoiceRepo.GetWithItemsAsync(id);
            if (invoice == null) return;

            _invoiceRepo.Remove(invoice);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<decimal> GetTodayTotalAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _context.PurchaseInvoices
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.CreatedAt >= today)
                .SumAsync(x => x.Total);
        }

        public async Task<int> GetTodayCountAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _context.PurchaseInvoices
                .AsNoTracking()
                .CountAsync(x => !x.IsDeleted && x.CreatedAt >= today);
        }

        private async Task<string> GenerateInvoiceNumberAsync()
        {
            var today = DateTime.UtcNow.ToString("yyyyMMdd");
            var prefix = $"PUR-{today}";

            var count = await _context.PurchaseInvoices
                .AsNoTracking()
                .CountAsync(x => x.InvoiceNumber.StartsWith(prefix) && !x.IsDeleted);

            return $"{prefix}-{(count + 1):D4}";
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

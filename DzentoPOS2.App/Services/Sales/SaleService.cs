using DzentoPOS2.App.Services.Sales;
using DzentoPOS2.Core.Entities.Sales;
using DzentoPOS2.Core.ValueObjects.Sales;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common.Interfaces;
using DzentoPOS2.Data.Repositories.Sales.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Sales;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.App.Services.Sales
{
    public class SaleService : ISaleService
    {
        private readonly ISalesInvoiceRepository _invoiceRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppDbContext _context;

        public SaleService(
            ISalesInvoiceRepository invoiceRepo,
            IUnitOfWork unitOfWork,
            AppDbContext context)
        {
            _invoiceRepo = invoiceRepo;
            _unitOfWork = unitOfWork;
            _context = context;
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

            var invoiceNumber = await GenerateInvoiceNumberAsync();

            var invoice = SalesInvoice.Create(
                invoiceNumber,
                dto.CustomerId,
                Guid.Empty,
                dto.Items.Sum(i => i.Quantity * i.Price),
                dto.Discount,
                dto.TaxRate,
                dto.PaidAmount);

            foreach (var item in dto.Items)
            {
                invoice.Items.Add(new SalesInvoiceItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    Total = item.Quantity * item.Price,
                    Description = item.Description ?? string.Empty
                });
            }

            await _context.SalesInvoices.AddAsync(invoice);

            if (dto.PaidAmount > 0)
            {
                await _context.SalesPayments.AddAsync(new SalesPayment
                {
                    SalesInvoiceId = invoice.Id,
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
            return await _context.SalesInvoices
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.CreatedAt >= today)
                .SumAsync(x => x.Total.Amount);
        }

        public async Task<int> GetTodayCountAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _context.SalesInvoices
                .AsNoTracking()
                .CountAsync(x => !x.IsDeleted && x.CreatedAt >= today);
        }

        private async Task<string> GenerateInvoiceNumberAsync()
        {
            var today = DateTime.UtcNow.ToString("yyyyMMdd");
            var prefix = $"INV-{today}";

            var count = await _context.SalesInvoices
                .AsNoTracking()
                .CountAsync(x => x.InvoiceNumber.Value.StartsWith(prefix) && !x.IsDeleted);

            return $"{prefix}-{(count + 1):D4}";
        }

        private static SaleDto Map(SalesInvoice invoice)
        {
            return new SaleDto
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber?.Value ?? "",
                CustomerId = invoice.CustomerId,
                Total = invoice.Total?.Amount ?? 0,
                Discount = invoice.Discount?.Value ?? 0,
                TaxRate = invoice.Tax?.Rate ?? 0,
                PaidAmount = invoice.PaidAmount?.Amount ?? 0,
                RemainingAmount = invoice.RemainingAmount?.Amount ?? 0,
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

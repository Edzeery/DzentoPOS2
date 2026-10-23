using DzentoPOS2.App.Services.Partners;
using DzentoPOS2.Core.Entities.Partners;
using DzentoPOS2.Core.ValueObjects.Business;
using DzentoPOS2.Core.ValueObjects.Identity;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common.Interfaces;
using DzentoPOS2.Data.Repositories.Partners.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Partners;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.App.Services.Partners
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppDbContext _context;

        public CustomerService(
            ICustomerRepository customerRepo,
            IUnitOfWork unitOfWork,
            AppDbContext context)
        {
            _customerRepo = customerRepo;
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<List<CustomerDto>> GetAllAsync()
        {
            var customers = await _context.Customers
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.FullName)
                .ToListAsync();

            return customers.Select(Map).ToList();
        }

        public async Task<CustomerDto?> GetByIdAsync(Guid id)
        {
            var customer = await _customerRepo.GetByIdAsync(id);
            return customer == null ? null : Map(customer);
        }

        public async Task<List<CustomerDto>> SearchAsync(string? term)
        {
            var query = _context.Customers
                .AsNoTracking()
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(term))
            {
                var lower = term.ToLower();
                query = query.Where(x =>
                    x.FullName.ToLower().Contains(lower) ||
                    (x.PhoneNumber != null && x.PhoneNumber.Value.Contains(term)));
            }

            var customers = await query
                .OrderBy(x => x.FullName)
                .ToListAsync();

            return customers.Select(Map).ToList();
        }

        public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto)
        {
            var customer = new Customer
            {
                FullName = dto.FullName,
                PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber)
                    ? null
                    : new PhoneNumber(dto.PhoneNumber),
                Address = string.IsNullOrWhiteSpace(dto.City)
                    ? null
                    : new Address(dto.Country ?? "", dto.City, dto.Street ?? ""),
                IsActive = dto.IsActive
            };

            await _customerRepo.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            return Map(customer);
        }

        public async Task<CustomerDto> UpdateAsync(UpdateCustomerDto dto)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(x => x.Id == dto.Id && !x.IsDeleted)
                ?? throw new KeyNotFoundException("Customer not found");

            customer.FullName = dto.FullName;
            customer.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber)
                ? null
                : new PhoneNumber(dto.PhoneNumber);
            customer.Address = string.IsNullOrWhiteSpace(dto.City)
                ? null
                : new Address(dto.Country ?? "", dto.City, dto.Street ?? "");
            customer.IsActive = dto.IsActive;

            _customerRepo.Update(customer);
            await _unitOfWork.SaveChangesAsync();

            return Map(customer);
        }

        public async Task DeleteAsync(Guid id)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (customer == null) return;

            _customerRepo.Remove(customer);
            await _unitOfWork.SaveChangesAsync();
        }

        private static CustomerDto Map(Customer c) => new()
        {
            Id = c.Id,
            FullName = c.FullName,
            PhoneNumber = c.PhoneNumber?.Value,
            Country = c.Address?.Country,
            City = c.Address?.City,
            Street = c.Address?.Street,
            Balance = c.Balance,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt
        };
    }
}

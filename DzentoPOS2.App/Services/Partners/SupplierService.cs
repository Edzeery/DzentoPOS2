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
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepository _supplierRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppDbContext _context;

        public SupplierService(
            ISupplierRepository supplierRepo,
            IUnitOfWork unitOfWork,
            AppDbContext context)
        {
            _supplierRepo = supplierRepo;
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<List<SupplierDto>> GetAllAsync()
        {
            var suppliers = await _context.Suppliers
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Name)
                .ToListAsync();

            return suppliers.Select(Map).ToList();
        }

        public async Task<SupplierDto?> GetByIdAsync(Guid id)
        {
            var supplier = await _supplierRepo.GetByIdAsync(id);
            return supplier == null ? null : Map(supplier);
        }

        public async Task<List<SupplierDto>> SearchAsync(string? term)
        {
            var query = _context.Suppliers
                .AsNoTracking()
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(term))
            {
                var lower = term.ToLower();
                query = query.Where(x =>
                    x.Name.ToLower().Contains(lower) ||
                    (x.PhoneNumber != null && x.PhoneNumber.Value.Contains(term)));
            }

            var suppliers = await query
                .OrderBy(x => x.Name)
                .ToListAsync();

            return suppliers.Select(Map).ToList();
        }

        public async Task<SupplierDto> CreateAsync(CreateSupplierDto dto)
        {
            var supplier = new Supplier
            {
                Name = dto.Name,
                PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber)
                    ? null
                    : new PhoneNumber(dto.PhoneNumber),
                Address = string.IsNullOrWhiteSpace(dto.City)
                    ? null
                    : new Address(dto.Country ?? "", dto.City, dto.Street ?? ""),
                IsActive = dto.IsActive
            };

            await _supplierRepo.AddAsync(supplier);
            await _unitOfWork.SaveChangesAsync();

            return Map(supplier);
        }

        public async Task<SupplierDto> UpdateAsync(UpdateSupplierDto dto)
        {
            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(x => x.Id == dto.Id && !x.IsDeleted)
                ?? throw new KeyNotFoundException("Supplier not found");

            supplier.Name = dto.Name;
            supplier.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber)
                ? null
                : new PhoneNumber(dto.PhoneNumber);
            supplier.Address = string.IsNullOrWhiteSpace(dto.City)
                ? null
                : new Address(dto.Country ?? "", dto.City, dto.Street ?? "");
            supplier.IsActive = dto.IsActive;

            _supplierRepo.Update(supplier);
            await _unitOfWork.SaveChangesAsync();

            return Map(supplier);
        }

        public async Task DeleteAsync(Guid id)
        {
            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (supplier == null) return;

            _supplierRepo.Remove(supplier);
            await _unitOfWork.SaveChangesAsync();
        }

        private static SupplierDto Map(Supplier s) => new()
        {
            Id = s.Id,
            Name = s.Name,
            PhoneNumber = s.PhoneNumber?.Value,
            Country = s.Address?.Country,
            City = s.Address?.City,
            Street = s.Address?.Street,
            Balance = s.Balance,
            IsActive = s.IsActive,
            CreatedAt = s.CreatedAt
        };
    }
}

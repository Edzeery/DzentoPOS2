using AutoMapper;
using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Inventory.Interfaces;
using DzentoPOS2.Data.Repositories.Common.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Inventory;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.App.Services.Inventory;

public class ProductUnitService : IProductUnitService
{
    private readonly IProductUnitRepository _unitRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;

    public ProductUnitService(
        IProductUnitRepository unitRepo,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        AppDbContext context)
    {
        _unitRepo = unitRepo;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _context = context;
    }

    public async Task<List<ProductUnitDto>> GetAllAsync()
    {
        var units = await _unitRepo.GetAllAsync();
        return _mapper.Map<List<ProductUnitDto>>(units);
    }

    public async Task<ProductUnitDto?> GetByIdAsync(Guid id)
    {
        var unit = await _unitRepo.GetByIdAsync(id);
        return unit == null ? null : _mapper.Map<ProductUnitDto>(unit);
    }

    public async Task<List<ProductUnitDto>> SearchAsync(string? term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return await GetAllAsync();

        var units = await _unitRepo.FindAsync(u =>
            u.Name.Contains(term) ||
            u.Symbol.Contains(term));
        return _mapper.Map<List<ProductUnitDto>>(units);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null)
    {
        if (excludeId.HasValue)
        {
            var existing = await _unitRepo.FindAsync(u =>
                u.Name == name &&
                u.Id != excludeId.Value);
            return existing.Any();
        }

        return await _unitRepo.ExistsAsync(u => u.Name == name);
    }

    public async Task<bool> ExistsBySymbolAsync(string symbol, Guid? excludeId = null)
    {
        if (excludeId.HasValue)
        {
            var existing = await _unitRepo.FindAsync(u =>
                u.Symbol == symbol &&
                u.Id != excludeId.Value);
            return existing.Any();
        }

        return await _unitRepo.ExistsAsync(u => u.Symbol == symbol);
    }

    public async Task<List<ProductUnitDto>> GetBaseUnitsAsync()
    {
        var units = await _unitRepo.GetBaseUnitsAsync();
        return _mapper.Map<List<ProductUnitDto>>(units);
    }

    public async Task<ProductUnitDto> CreateAsync(CreateProductUnitDto dto)
    {
        if (await ExistsByNameAsync(dto.Name))
            throw new InvalidOperationException($"A unit named '{dto.Name}' already exists.");

        if (await ExistsBySymbolAsync(dto.Symbol))
            throw new InvalidOperationException($"A unit with symbol '{dto.Symbol}' already exists.");

        var unit = new ProductUnit
        {
            Name = dto.Name.Trim(),
            Symbol = dto.Symbol.Trim(),
            IsBaseUnit = dto.IsBaseUnit
        };

        await _unitRepo.AddAsync(unit);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ProductUnitDto>(unit);
    }

    public async Task<ProductUnitDto> UpdateAsync(UpdateProductUnitDto dto)
    {
        var unit = await _context.ProductUnits
            .FirstOrDefaultAsync(u => u.Id == dto.Id && !u.IsDeleted);
        if (unit == null)
            throw new KeyNotFoundException($"Unit {dto.Id} not found.");

        if (await ExistsByNameAsync(dto.Name, dto.Id))
            throw new InvalidOperationException($"A unit named '{dto.Name}' already exists.");

        if (await ExistsBySymbolAsync(dto.Symbol, dto.Id))
            throw new InvalidOperationException($"A unit with symbol '{dto.Symbol}' already exists.");

        unit.Name = dto.Name.Trim();
        unit.Symbol = dto.Symbol.Trim();
        unit.IsBaseUnit = dto.IsBaseUnit;

        _unitRepo.Update(unit);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ProductUnitDto>(unit);
    }

    public async Task DeleteAsync(Guid id)
    {
        var unit = await _context.ProductUnits
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        if (unit == null) return;

        _unitRepo.Remove(unit);
        await _unitOfWork.SaveChangesAsync();
    }
}

using AutoMapper;
using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Inventory.Interfaces;
using DzentoPOS2.Data.Repositories.Common.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Inventory;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.App.Services.Inventory;

public class ProductCategoryService : IProductCategoryService
{
    private readonly IProductCategoryRepository _categoryRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;

    public ProductCategoryService(
        IProductCategoryRepository categoryRepo,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        AppDbContext context)
    {
        _categoryRepo = categoryRepo;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _context = context;
    }

    public async Task<List<ProductCategoryDto>> GetAllAsync()
    {
        var categories = await _categoryRepo.GetAllAsync();
        return _mapper.Map<List<ProductCategoryDto>>(categories);
    }

    public async Task<ProductCategoryDto?> GetByIdAsync(Guid id)
    {
        var category = await _categoryRepo.GetByIdAsync(id);
        return category == null ? null : _mapper.Map<ProductCategoryDto>(category);
    }

    public async Task<List<ProductCategoryDto>> SearchAsync(string? term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return await GetAllAsync();

        var categories = await _categoryRepo.FindAsync(c =>
            c.Name.Contains(term) ||
            (c.Description != null && c.Description.Contains(term)));
        return _mapper.Map<List<ProductCategoryDto>>(categories);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null)
    {
        if (excludeId.HasValue)
        {
            var existing = await _categoryRepo.FindAsync(c =>
                c.Name == name &&
                c.Id != excludeId.Value &&
                !c.IsDeleted);
            return existing.Any();
        }

        return await _categoryRepo.ExistsAsync(c =>
            c.Name == name &&
            !c.IsDeleted);
    }

    public async Task<ProductCategoryDto> CreateAsync(CreateProductCategoryDto dto)
    {
        if (await ExistsByNameAsync(dto.Name))
            throw new InvalidOperationException($"A category named '{dto.Name}' already exists.");

        var category = new ProductCategory
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            IsActive = dto.IsActive
        };

        await _categoryRepo.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ProductCategoryDto>(category);
    }

    public async Task<ProductCategoryDto> UpdateAsync(UpdateProductCategoryDto dto)
    {
        var category = await _context.ProductCategories
            .FirstOrDefaultAsync(c => c.Id == dto.Id && !c.IsDeleted);
        if (category == null)
            throw new KeyNotFoundException($"Category {dto.Id} not found.");

        if (await ExistsByNameAsync(dto.Name, dto.Id))
            throw new InvalidOperationException($"A category named '{dto.Name}' already exists.");

        category.Name = dto.Name.Trim();
        category.Description = dto.Description?.Trim();
        category.IsActive = dto.IsActive;

        _categoryRepo.Update(category);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ProductCategoryDto>(category);
    }

    public async Task DeleteAsync(Guid id)
    {
        var category = await _context.ProductCategories
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (category == null) return;

        _categoryRepo.Remove(category);
        await _unitOfWork.SaveChangesAsync();
    }
}

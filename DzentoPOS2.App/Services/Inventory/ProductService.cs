using AutoMapper;
using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.App.Session;
using DzentoPOS2.Core.Entities.Businesses;
using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Core.ValueObjects.Common;
using DzentoPOS2.Core.ValueObjects.Inventory;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Businesses.Interfaces;
using DzentoPOS2.Data.Repositories.Common.Interfaces;
using DzentoPOS2.Data.Repositories.Inventory.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Inventory;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.App.Services.Inventory;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepo;
    private readonly IProductCategoryRepository _categoryRepo;
    private readonly IProductUnitRepository _unitRepo;
    private readonly IProductStockRepository _stockRepo;
    private readonly IWarehouseRepository _warehouseRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;
    private readonly IStockService _stockService;
    private readonly IStoreContextService _storeContext;
    private readonly ICurrentUserService _currentUser;

    public ProductService(
        IProductRepository productRepo,
        IProductCategoryRepository categoryRepo,
        IProductUnitRepository unitRepo,
        IProductStockRepository stockRepo,
        IWarehouseRepository warehouseRepo,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        AppDbContext context,
        IStockService stockService,
        IStoreContextService storeContext,
        ICurrentUserService currentUser)
    {
        _productRepo = productRepo;
        _categoryRepo = categoryRepo;
        _unitRepo = unitRepo;
        _stockRepo = stockRepo;
        _warehouseRepo = warehouseRepo;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _context = context;
        _stockService = stockService;
        _storeContext = storeContext;
        _currentUser = currentUser;
    }

    public async Task<List<ProductDto>> GetAllAsync()
    {
        var products = await _productRepo.GetAllAsync();
        return await MapWithCategoryAndUnit(products);
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id)
    {
        var product = await _productRepo.GetWithDetailsAsync(id);
        return product == null ? null : MapProduct(product);
    }

    public async Task<ProductDto?> GetBySkuAsync(string sku)
    {
        var product = await _productRepo.GetBySkuAsync(sku);
        return product == null ? null : MapProduct(product);
    }

    public async Task<List<ProductDto>> SearchAsync(string? term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return await GetAllAsync();

        var products = await _productRepo.FindAsync(p =>
            p.Name.Contains(term) ||
            (p.SKU != null && p.SKU.Contains(term)));
        return await MapWithCategoryAndUnit(products);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            SKU = dto.SKU,
            Barcode = string.IsNullOrWhiteSpace(dto.Barcode) ? null : dto.Barcode.Trim(),
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            UnitId = dto.UnitId,
            CostPrice = new Money(dto.CostPrice, "SAR"),
            SalePrice = new Money(dto.SalePrice, "SAR"),
            MinimumStock = new Quantity(dto.MinimumStock),
            TrackStock = dto.TrackStock,
            IsActive = dto.IsActive
        };

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            await _productRepo.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            if (product.TrackStock && dto.OpeningStock > 0)
            {
                var branchId = await _storeContext.GetDefaultBranchIdAsync();
                var warehouseId = await _storeContext
                    .GetDefaultWarehouseIdAsync(branchId);

                await _stockService.AdjustAsync(
                    product.Id,
                    warehouseId,
                    dto.OpeningStock,
                    "Opening stock",
                    _currentUser.UserId,
                    isOpening: true);

                await _unitOfWork.SaveChangesAsync();
            }

            await _unitOfWork.CommitTransactionAsync();

            return MapProduct(product);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            _context.ChangeTracker.Clear();
            throw;
        }
    }

    public async Task<ProductDto> UpdateAsync(UpdateProductDto dto)
    {
        // The shared context can still track this product from an earlier read.
        // Reading it tracked (instead of attaching a detached copy) avoids the
        // "another instance with the same key is already being tracked" error.
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .FirstOrDefaultAsync(p => p.Id == dto.Id && !p.IsDeleted);
        if (product == null)
            throw new KeyNotFoundException($"Product {dto.Id} not found");

        product.Name = dto.Name;
        product.SKU = dto.SKU;
        product.Barcode = string.IsNullOrWhiteSpace(dto.Barcode) ? null : dto.Barcode.Trim();
        product.Description = dto.Description;
        product.CategoryId = dto.CategoryId;
        product.UnitId = dto.UnitId;
        product.CostPrice = new Money(dto.CostPrice, "SAR");
        product.SalePrice = new Money(dto.SalePrice, "SAR");
        product.MinimumStock = new Quantity(dto.MinimumStock);
        product.TrackStock = dto.TrackStock;
        product.IsActive = dto.IsActive;

        _productRepo.Update(product);
        await _unitOfWork.SaveChangesAsync();

        return MapProduct(product);
    }

    public async Task DeleteAsync(Guid id)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        if (product == null) return;
        _productRepo.Remove(product);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<List<ProductCategoryDto>> GetCategoriesAsync()
    {
        var categories = await _categoryRepo.FindAsync(c => c.IsActive);
        return _mapper.Map<List<ProductCategoryDto>>(categories);
    }

    public async Task<List<ProductUnitDto>> GetUnitsAsync()
    {
        var units = await _unitRepo.GetAllAsync();
        return _mapper.Map<List<ProductUnitDto>>(units);
    }

    public async Task<List<WarehouseDto>> GetWarehousesAsync()
    {
        var warehouses = await _warehouseRepo.FindAsync(w => w.IsActive);
        return _mapper.Map<List<WarehouseDto>>(warehouses);
    }

    public async Task<ProductInventoryStatsDto> GetInventoryStatsAsync()
    {
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        var total = await _context.Products.CountAsync(p => !p.IsDeleted);
        var active = await _context.Products.CountAsync(p => p.IsActive && !p.IsDeleted);
        var lastMonth = await _context.Products
            .CountAsync(p => !p.IsDeleted && p.CreatedAt < monthStart);

        var stockData = await (from stock in _context.ProductStocks
                               join p in _context.Products on stock.ProductId equals p.Id
                               where !p.IsDeleted
                               select new
                               {
                                   p.Id,
                                   Quantity = stock.Quantity.Value,
                                   MinStock = p.MinimumStock != null ? p.MinimumStock.Value : (decimal?)null,
                                   CostAmount = p.CostPrice != null ? p.CostPrice.Amount : (decimal?)null
                               }).ToListAsync();

        var lowStock = stockData
            .Where(x => x.MinStock.HasValue)
            .GroupBy(x => new { x.Id, x.MinStock })
            .Where(g => g.Sum(x => x.Quantity) < g.Key.MinStock!.Value)
            .Count();

        var stockValue = stockData.Sum(x => x.Quantity * (x.CostAmount ?? 0));

        return new ProductInventoryStatsDto
        {
            TotalProducts = total,
            ActiveProducts = active,
            LowStockProducts = lowStock,
            TotalStockValue = stockValue,
            LastMonthProducts = lastMonth
        };
    }

    public async Task AdjustStockAsync(
        Guid productId,
        decimal targetQuantity,
        string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("An adjustment reason is required", nameof(reason));

        var branchId = await _storeContext.GetDefaultBranchIdAsync();
        var warehouseId = await _storeContext.GetDefaultWarehouseIdAsync(branchId);

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            await _stockService.AdjustAsync(
                productId,
                warehouseId,
                targetQuantity,
                reason,
                _currentUser.UserId,
                isOpening: false);

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

    public async Task<List<ProductStockDto>> GetProductStocksAsync(Guid productId)
    {
        var stocks = await _context.ProductStocks
            .Where(s => s.ProductId == productId)
            .Include(s => s.Warehouse)
            .ToListAsync();

        return stocks.Select(s => new ProductStockDto
        {
            WarehouseId = s.WarehouseId,
            WarehouseName = s.Warehouse?.Name ?? "",
            Quantity = s.Quantity?.Value ?? 0
        }).ToList();
    }

    private Task<List<ProductDto>> MapWithCategoryAndUnit(IEnumerable<Product> products)
    {
        var result = new List<ProductDto>();
        foreach (var p in products)
            result.Add(MapProduct(p));
        return Task.FromResult(result);
    }

    private ProductDto MapProduct(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        SKU = product.SKU,
        Barcode = product.Barcode,
        Description = product.Description,
        CategoryId = product.CategoryId,
        CategoryName = product.Category?.Name ?? "",
        UnitId = product.UnitId,
        UnitName = product.Unit?.Name ?? "",
        CostPrice = product.CostPrice?.Amount ?? 0,
        SalePrice = product.SalePrice?.Amount ?? 0,
        MinimumStock = product.MinimumStock?.Value ?? 0,
        TrackStock = product.TrackStock,
        IsActive = product.IsActive,
        Currency = product.CostPrice?.Currency ?? "SAR"
    };
}

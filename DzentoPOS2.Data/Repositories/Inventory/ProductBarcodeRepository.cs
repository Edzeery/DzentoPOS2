using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common;
using DzentoPOS2.Data.Repositories.Inventory.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Inventory
{
    public class ProductBarcodeRepository
        : Repository<ProductBarcode>,
        IProductBarcodeRepository
    {
        public ProductBarcodeRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<ProductBarcode?> GetByBarcodeAsync(string barcode)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(x => x.Product)
                .FirstOrDefaultAsync(x =>
                    x.Barcode == barcode &&
                    !x.IsDeleted);
        }

        public async Task<IEnumerable<ProductBarcode>> GetByProductAsync(Guid productId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(x =>
                    x.ProductId == productId &&
                    !x.IsDeleted)
                .ToListAsync();
        }
    }
}
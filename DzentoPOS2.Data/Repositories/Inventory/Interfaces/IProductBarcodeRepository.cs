using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Data.Repositories.Common.Interfaces;

namespace DzentoPOS2.Data.Repositories.Inventory.Interfaces
{
    public interface IProductBarcodeRepository
        : IRepository<ProductBarcode>
    {
        Task<ProductBarcode?> GetByBarcodeAsync(string barcode);

        Task<IEnumerable<ProductBarcode>> GetByProductAsync(Guid productId);
    }
}
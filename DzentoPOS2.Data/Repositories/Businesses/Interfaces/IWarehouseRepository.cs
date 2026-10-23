using DzentoPOS2.Core.Entities.Businesses;
using DzentoPOS2.Data.Repositories.Common.Interfaces;

namespace DzentoPOS2.Data.Repositories.Businesses.Interfaces
{
    public interface IWarehouseRepository
        : IRepository<Warehouse>
    {
        Task<Warehouse?> GetWithBranchAsync(Guid id);

        Task<IEnumerable<Warehouse>> GetByBranchAsync(Guid branchId);

        Task<bool> ExistsByNameAsync(string name);
    }
}
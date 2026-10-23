using DzentoPOS2.Core.Entities.Businesses;
using DzentoPOS2.Data.Repositories.Common.Interfaces;

namespace DzentoPOS2.Data.Repositories.Businesses.Interfaces
{
    public interface IBranchRepository : IRepository<Branch>
    {
        Task<Branch?> GetWithDetailsAsync(Guid id);

        Task<IEnumerable<Branch>> GetActiveBranchesAsync();

        Task<bool> ExistsByNameAsync(string name);
    }
}
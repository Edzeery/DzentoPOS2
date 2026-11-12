using DzentoPOS2.Core.Entities.Businesses;
using DzentoPOS2.Data.Repositories.Common.Interfaces;

namespace DzentoPOS2.Data.Repositories.Businesses.Interfaces
{
    public interface IBranchRepository : IRepository<Branch>
    {
        Task<Branch?> GetWithDetailsAsync(Guid id);

        Task<IEnumerable<Branch>> GetActiveBranchesAsync();

        Task<bool> ExistsByNameAsync(string name);

        /// <summary>
        /// Returns every non-deleted branch ordered so the main branch comes
        /// first, then by creation order. Used by the branch management list.
        /// </summary>
        Task<IReadOnlyList<Branch>> GetAllOrderedAsync();

        /// <summary>
        /// True when any cashbox that belongs to the branch has an open cash
        /// session. A branch in this state must not be deactivated.
        /// </summary>
        Task<bool> HasOpenCashSessionAsync(Guid branchId);
    }
}
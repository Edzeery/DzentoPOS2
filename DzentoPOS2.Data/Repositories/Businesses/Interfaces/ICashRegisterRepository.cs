using DzentoPOS2.Core.Entities.Businesses;
using DzentoPOS2.Data.Repositories.Common.Interfaces;

namespace DzentoPOS2.Data.Repositories.Businesses.Interfaces
{
    public interface ICashRegisterRepository
        : IRepository<CashRegister>
    {
        Task<CashRegister?> GetWithBranchAsync(Guid id);

        Task<IEnumerable<CashRegister>> GetByBranchAsync(Guid branchId);

        Task<bool> ExistsByCodeAsync(string code);
    }
}
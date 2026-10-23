using DzentoPOS2.Core.Entities.Accounting;
using DzentoPOS2.Data.Repositories.Common.Interfaces;

namespace DzentoPOS2.Data.Repositories.Accounting.Interfaces
{
    public interface ICashboxRepository
        : IRepository<Cashbox>
    {
        Task<Cashbox?> GetByNameAsync(string name);

        Task<IEnumerable<Cashbox>> GetActiveAsync();
    }
}
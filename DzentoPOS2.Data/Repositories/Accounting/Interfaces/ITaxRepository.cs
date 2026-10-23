using DzentoPOS2.Core.Entities.Accounting;
using DzentoPOS2.Data.Repositories.Common.Interfaces;

namespace DzentoPOS2.Data.Repositories.Accounting.Interfaces
{
    public interface ITaxRepository
        : IRepository<Tax>
    {
        Task<IEnumerable<Tax>> GetActiveAsync();
    }
}
using DzentoPOS2.Core.Entities.Accounting;
using DzentoPOS2.Data.Repositories.Common.Interfaces;

namespace DzentoPOS2.Data.Repositories.Accounting.Interfaces
{
    public interface IFinancialPeriodRepository
        : IRepository<FinancialPeriod>
    {
        Task<FinancialPeriod?> GetOpenPeriodAsync();
    }
}
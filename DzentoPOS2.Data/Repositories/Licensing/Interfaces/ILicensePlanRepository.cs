using DzentoPOS2.Core.Entities.Licensing;
using DzentoPOS2.Data.Repositories.Common.Interfaces;

namespace DzentoPOS2.Data.Repositories.Licensing.Interfaces
{
    public interface ILicensePlanRepository
        : IRepository<LicensePlan>
    {
        Task<LicensePlan?> GetByNameAsync(string name);

        Task<IEnumerable<LicensePlan>> GetActivePlansAsync();
    }
}
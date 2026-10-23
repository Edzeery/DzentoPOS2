using DzentoPOS2.Core.Entities.Licensing;
using DzentoPOS2.Data.Repositories.Common.Interfaces;

namespace DzentoPOS2.Data.Repositories.Licensing.Interfaces
{
    public interface ILicenseRepository
        : IRepository<License>
    {
        Task<License?> GetByKeyAsync(string licenseKey);

        Task<IEnumerable<License>> GetActiveLicensesAsync();

        Task<IEnumerable<License>> GetExpiredLicensesAsync();
    }
}
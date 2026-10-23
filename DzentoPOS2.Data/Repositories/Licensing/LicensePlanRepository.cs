using DzentoPOS2.Core.Entities.Licensing;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common;
using DzentoPOS2.Data.Repositories.Licensing.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Licensing
{
    public class LicensePlanRepository
        : Repository<LicensePlan>,
        ILicensePlanRepository
    {
        public LicensePlanRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<LicensePlan?> GetByNameAsync(
            string name)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x =>
                    x.Name == name &&
                    !x.IsDeleted);
        }

        public async Task<IEnumerable<LicensePlan>> GetActivePlansAsync()
        {
            return await _dbSet
                .Where(x =>
                    x.IsActive &&
                    !x.IsDeleted)
                .ToListAsync();
        }
    }
}
using DzentoPOS2.Core.Entities.Licensing;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common;
using DzentoPOS2.Data.Repositories.Licensing.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Licensing
{
    public class LicenseRepository
        : Repository<License>,
        ILicenseRepository
    {
        public LicenseRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<License?> GetByKeyAsync(
            string licenseKey)
        {
            return await _dbSet
                .Include(x => x.LicensePlan)
                .Include(x => x.DeviceActivations)
                .FirstOrDefaultAsync(x =>
                    x.LicenseKey == licenseKey &&
                    !x.IsDeleted);
        }

        public async Task<IEnumerable<License>> GetActiveLicensesAsync()
        {
            return await _dbSet
                .Include(x => x.LicensePlan)
                .Where(x =>
                    x.IsActive &&
                    x.ExpiryDate >= DateTime.UtcNow &&
                    !x.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<License>> GetExpiredLicensesAsync()
        {
            return await _dbSet
                .Include(x => x.LicensePlan)
                .Where(x =>
                    x.ExpiryDate < DateTime.UtcNow &&
                    !x.IsDeleted)
                .ToListAsync();
        }
    }
}
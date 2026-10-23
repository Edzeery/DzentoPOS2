using DzentoPOS2.Core.Entities.Licensing;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common;
using DzentoPOS2.Data.Repositories.Licensing.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Licensing
{
    public class DeviceActivationRepository
        : Repository<DeviceActivation>,
        IDeviceActivationRepository
    {
        public DeviceActivationRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<DeviceActivation>> GetByLicenseAsync(
            Guid licenseId)
        {
            return await _dbSet
                .Include(x => x.License)
                .Where(x =>
                    x.LicenseId == licenseId &&
                    !x.IsDeleted)
                .OrderByDescending(x => x.ActivatedAt)
                .ToListAsync();
        }

        public async Task<DeviceActivation?> GetByDeviceIdentifierAsync(
            string deviceIdentifier)
        {
            return await _dbSet
                .Include(x => x.License)
                .FirstOrDefaultAsync(x =>
                    x.DeviceIdentifier == deviceIdentifier &&
                    !x.IsDeleted);
        }
    }
}
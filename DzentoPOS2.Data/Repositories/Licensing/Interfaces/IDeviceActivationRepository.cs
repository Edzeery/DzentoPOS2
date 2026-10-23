using DzentoPOS2.Core.Entities.Licensing;
using DzentoPOS2.Data.Repositories.Common.Interfaces;

namespace DzentoPOS2.Data.Repositories.Licensing.Interfaces
{
    public interface IDeviceActivationRepository
        : IRepository<DeviceActivation>
    {
        Task<IEnumerable<DeviceActivation>> GetByLicenseAsync(
            Guid licenseId);

        Task<DeviceActivation?> GetByDeviceIdentifierAsync(
            string deviceIdentifier);
    }
}
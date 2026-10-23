using DzentoPOS2.Core.Entities.Partners;
using DzentoPOS2.Core.ValueObjects.Identity;
using DzentoPOS2.Data.Repositories.Common.Interfaces;

namespace DzentoPOS2.Data.Repositories.Partners.Interfaces
{
    public interface ISupplierRepository : IRepository<Supplier>
    {
        Task<Supplier?> GetByPhoneAsync(PhoneNumber phoneNumber);

        Task<IEnumerable<Supplier>> GetActiveSuppliersAsync();

        Task<bool> ExistsByPhoneAsync(PhoneNumber phoneNumber);
    }
}
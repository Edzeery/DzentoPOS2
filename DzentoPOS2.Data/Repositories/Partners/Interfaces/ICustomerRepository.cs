using DzentoPOS2.Core.Entities.Partners;
using DzentoPOS2.Core.ValueObjects.Identity;
using DzentoPOS2.Data.Repositories.Common.Interfaces;

namespace DzentoPOS2.Data.Repositories.Partners.Interfaces
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        Task<Customer?> GetByPhoneAsync(PhoneNumber phoneNumber);

        Task<IEnumerable<Customer>> GetActiveCustomersAsync();

        Task<bool> ExistsByPhoneAsync(PhoneNumber phoneNumber);
    }
}
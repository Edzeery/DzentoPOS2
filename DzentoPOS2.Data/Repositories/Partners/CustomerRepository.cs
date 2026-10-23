using DzentoPOS2.Core.Entities.Partners;
using DzentoPOS2.Core.ValueObjects.Identity;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common;
using DzentoPOS2.Data.Repositories.Partners.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Partners
{
    public class CustomerRepository
        : Repository<Customer>,
          ICustomerRepository
    {
        public CustomerRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<Customer?> GetByPhoneAsync(PhoneNumber phoneNumber)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x =>
                    x.PhoneNumber == phoneNumber &&
                    !x.IsDeleted);
        }

        public async Task<IEnumerable<Customer>> GetActiveCustomersAsync()
        {
            return await _dbSet
                .Where(x =>
                    x.IsActive &&
                    !x.IsDeleted)
                .ToListAsync();
        }

        public async Task<bool> ExistsByPhoneAsync(PhoneNumber phoneNumber)
        {
            return await _dbSet
                .AnyAsync(x =>
                    x.PhoneNumber == phoneNumber &&
                    !x.IsDeleted);
        }
    }
}
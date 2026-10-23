using DzentoPOS2.Core.Entities.Partners;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common;
using DzentoPOS2.Data.Repositories.Partners.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Partners
{
    public class CustomerGroupRepository
        : Repository<CustomerGroup>,
          ICustomerGroupRepository
    {
        public CustomerGroupRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<CustomerGroup>> GetActiveGroupsAsync()
        {
            return await _dbSet 
                .ToListAsync();
        }
    }
}
using DzentoPOS2.Core.Entities.Partners;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common;
using DzentoPOS2.Data.Repositories.Partners.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Partners
{
    public class SupplierPaymentRepository
        : Repository<SupplierPayment>,
          ISupplierPaymentRepository
    {
        public SupplierPaymentRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<SupplierPayment>> GetBySupplierAsync(Guid supplierId)
        {
            return await _dbSet
                .Where(x =>
                    x.SupplierId == supplierId &&
                    !x.IsDeleted)
                .ToListAsync();
        }
    }
}
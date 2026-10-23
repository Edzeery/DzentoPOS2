using DzentoPOS2.Core.Entities.Purchases;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common;
using DzentoPOS2.Data.Repositories.Purchases.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Purchases
{
    public class PurchaseReturnItemRepository
        : Repository<PurchaseReturnItem>,
          IPurchaseReturnItemRepository
    {
        public PurchaseReturnItemRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<PurchaseReturnItem>> GetByReturnAsync(Guid returnId)
        {
            return await _dbSet
                .Where(x =>
                    x.PurchaseReturnId == returnId &&
                    !x.IsDeleted)
                .ToListAsync();
        }
    }
}
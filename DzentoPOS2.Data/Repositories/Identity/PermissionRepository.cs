using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DzentoPOS2.Core.Entities.Identity;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common;
using DzentoPOS2.Data.Repositories.Identity.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Identity
{
    public class PermissionRepository
        : Repository<Permission>,
          IPermissionRepository
    {
        public PermissionRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<Permission?> GetByCodeAsync(
            string code)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x =>
                    x.Code == code &&
                    !x.IsDeleted);
        }
    }
}
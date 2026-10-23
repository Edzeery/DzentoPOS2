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
    public class RoleRepository
        : Repository<Role>,
          IRoleRepository
    {
        public RoleRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<Role?> GetWithPermissionsAsync(
            Guid roleId)
        {
            return await _context.Roles
                .Include(x => x.RolePermissions)
                    .ThenInclude(x => x.Permission)
                .FirstOrDefaultAsync(x =>
                    x.Id == roleId &&
                    !x.IsDeleted);
        }
        public async Task<IReadOnlyList<Role>> GetPagedAsync(
                int pageNumber,
                int pageSize,
                string? search)
        {
            var query = _context.Roles
                .Include(x => x.RolePermissions)
                .AsQueryable();

            query = query.Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(x =>
                    x.Name.Contains(search));
            }

            return await query
                .OrderBy(x => x.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<int> CountAsync(string? search)
        {
            var query = _context.Roles
                .AsQueryable();

            query = query.Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(x =>
                    x.Name.Contains(search));
            }

            return await query.CountAsync();
        }
    }
}
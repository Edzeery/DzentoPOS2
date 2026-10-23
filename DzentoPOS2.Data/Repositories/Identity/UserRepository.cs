using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DzentoPOS2.Core.Entities.Identity;
using DzentoPOS2.Core.ValueObjects.Identity;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Common;
using DzentoPOS2.Data.Repositories.Identity.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Identity
{
    public class UserRepository
        : Repository<User>,
          IUserRepository
    {
        public UserRepository(AppDbContext context)
            : base(context)
        {
        }
       
        public async Task<User?> GetByUserNameAsync(
            string userName)
        {
            return await _dbSet
                .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                .FirstOrDefaultAsync(x =>
                    x.UserName == userName &&
                    !x.IsDeleted);
        }

        public async Task<User?> GetByEmailAsync(
            Email email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x =>
                    x.Email == email &&
                    !x.IsDeleted);
        }

        public async Task<User?> GetWithRolesAsync(
            Guid userId)
        {
            return await _dbSet
                .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                .FirstOrDefaultAsync(x =>
                    x.Id == userId &&
                    !x.IsDeleted);
        }

        public async Task<User?> GetByIdWithRolesAsync(Guid id)
        {
            return await _context.Users
                .AsNoTracking()
                .Include(x => x.UserRoles)
                    .ThenInclude(x => x.Role)
                        .ThenInclude(x => x!.RolePermissions)
                            .ThenInclude(x => x.Permission)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);
        }

        public async Task<IReadOnlyList<User>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? search)
        {
            var query = _context.Users
                .Include(x => x.Branch)
                .AsQueryable();

            query = query.Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(x =>
                    x.FullName.Contains(search) ||
                    x.UserName.Contains(search));
            }

            return await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> CountAsync(
            string? search)
        {
            var query = _context.Users
                .AsQueryable();

            query = query.Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(x =>
                    x.FullName.Contains(search) ||
                    x.UserName.Contains(search));
            }

            return await query.CountAsync();
        }
    }
}
using DzentoPOS2.Core.Entities.Identity;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Identity.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Identity
{
    public class UserRoleRepository
        : IUserRoleRepository
    {
        private readonly AppDbContext _context;

        public UserRoleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(UserRole userRole)
        {
            await _context.UserRoles.AddAsync(userRole);
        }

        public async Task RemoveAsync(
            Guid userId,
            Guid roleId)
        {
            var entity = await _context.UserRoles
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.RoleId == roleId);

            if (entity is not null)
            {
                _context.UserRoles.Remove(entity);
            }
        }

        public async Task<UserRole?> GetByUserIdAsync(
            Guid userId)
        {
            return await _context.UserRoles
                .AsNoTracking()
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    !x.IsDeleted);
        }
    }
}
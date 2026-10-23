using DzentoPOS2.Core.Entities.Identity;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Identity.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.Data.Repositories.Identity
{

    public class RolePermissionRepository
        : IRolePermissionRepository
    {

        private readonly AppDbContext _context;

        public RolePermissionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            RolePermission rolePermission)
        {
            await _context.RolePermissions
                .AddAsync(rolePermission);
        }

        public async Task RemoveAsync(
            Guid roleId,
            Guid permissionId)
        {
            var entity = await _context.RolePermissions
                .FirstOrDefaultAsync(x =>
                    x.RoleId == roleId &&
                    x.PermissionId == permissionId);

            if (entity is not null)
            {
                _context.RolePermissions.Remove(entity);
            }
        }

        public async Task<List<RolePermission>> GetByRoleIdAsync(
            Guid roleId)
        {
            return await _context.RolePermissions
                .Include(x => x.Permission)
                .Where(x => x.RoleId == roleId)
                .ToListAsync();
        }
    }
}
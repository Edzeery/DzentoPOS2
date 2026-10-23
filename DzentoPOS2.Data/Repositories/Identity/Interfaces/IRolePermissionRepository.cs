using DzentoPOS2.Core.Entities.Identity;
using DzentoPOS2.Data.Repositories.Common.Interfaces;

namespace DzentoPOS2.Data.Repositories.Identity.Interfaces
{
    public interface IRolePermissionRepository 
    {
        Task AddAsync(RolePermission rolePermission);

        Task RemoveAsync(Guid roleId, Guid permissionId);

        Task<List<RolePermission>> GetByRoleIdAsync(Guid roleId);

  
    }
}
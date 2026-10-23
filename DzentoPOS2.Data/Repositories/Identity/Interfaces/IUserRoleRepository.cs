using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DzentoPOS2.Core.Entities.Identity;

namespace DzentoPOS2.Data.Repositories.Identity.Interfaces
{
    public interface IUserRoleRepository
    {
        Task AddAsync(UserRole userRole);

        Task RemoveAsync(Guid userId, Guid roleId);

        //Task<List<UserRole>> GetByUserIdAsync(Guid userId);
        Task<UserRole?> GetByUserIdAsync(Guid userId);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DzentoPOS2.Core.Entities.Identity;
using DzentoPOS2.Data.Repositories.Common.Interfaces;

namespace DzentoPOS2.Data.Repositories.Identity.Interfaces
{
    public interface IRoleRepository
        : IRepository<Role>
    { 

        Task<Role?> GetWithPermissionsAsync(Guid roleId);

        Task<IReadOnlyList<Role>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? search);

        Task<int> CountAsync(
            string? search);
    }
}
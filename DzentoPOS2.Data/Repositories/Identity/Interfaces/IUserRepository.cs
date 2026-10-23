using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DzentoPOS2.Core.Entities.Identity;
using DzentoPOS2.Core.ValueObjects.Identity;
using DzentoPOS2.Data.Repositories.Common.Interfaces;

namespace DzentoPOS2.Data.Repositories.Identity.Interfaces
{
    public interface IUserRepository
        : IRepository<User>
    {
        Task<User?> GetByUserNameAsync(string userName);

        Task<User?> GetByEmailAsync(Email email);

        Task<User?> GetWithRolesAsync(Guid userId);
        Task<User?> GetByIdWithRolesAsync(Guid id);


        Task<IReadOnlyList<User>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? search);

        Task<int> CountAsync(string? search);
    }
}
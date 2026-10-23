using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DzentoPOS2.Core.Entities.Identity;
using DzentoPOS2.Data.Repositories.Common.Interfaces;

namespace DzentoPOS2.Data.Repositories.Identity.Interfaces
{
    public interface IPermissionRepository
        : IRepository<Permission>
    {
        Task<Permission?> GetByCodeAsync(string code);
    }
}
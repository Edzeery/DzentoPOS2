using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DzentoPOS2.App.Authorization
{
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(string permissionCode);

        Task<HashSet<string>> GetUserPermissionsAsync(Guid userId);

        Task RefreshCacheAsync(Guid userId);


    }
}

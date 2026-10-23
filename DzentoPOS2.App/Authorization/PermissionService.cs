using DzentoPOS2.App.Session;
using DzentoPOS2.Data.Repositories.Identity.Interfaces;

namespace DzentoPOS2.App.Authorization
{
    public class PermissionService : IPermissionService
    {
        private readonly ICurrentUserService _currentUser;
        private readonly IUserRepository _userRepository;

        private HashSet<string> _permissionsCache = new();

        public PermissionService(
            ICurrentUserService currentUser,
            IUserRepository userRepository)
        {
            _currentUser = currentUser;
            _userRepository = userRepository;
        }

        public async Task<bool> HasPermissionAsync(string permissionCode)
        {
            if (!_currentUser.IsAuthenticated)
                return false;

            if (_permissionsCache.Count == 0)
                await LoadPermissions();

            return _permissionsCache.Contains(permissionCode);
        }

        public async Task<HashSet<string>> GetUserPermissionsAsync(Guid userId)
        {
            var user =
                await _userRepository
                    .GetByIdWithRolesAsync(userId);

            if (user == null)
            {
                return new HashSet<string>();
            }

            return user.UserRoles
                .SelectMany(x => x.Role.RolePermissions)
                .Select(x => x.Permission.Code)
                .ToHashSet();
        }

        public async Task RefreshCacheAsync(Guid userId)
        {
            _permissionsCache = await GetUserPermissionsAsync(userId);
        }

        private async Task LoadPermissions()
        {
            if (_currentUser.UserId is null)
                return;

            _permissionsCache =
                await GetUserPermissionsAsync(_currentUser.UserId.Value);
        }
    }
}
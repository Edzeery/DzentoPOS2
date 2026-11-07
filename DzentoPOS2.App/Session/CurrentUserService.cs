using DzentoPOS2.Core.Services;
using DzentoPOS2.Shared.Contracts.DTOs.Identity.Auth;

namespace DzentoPOS2.App.Session
{
    public class CurrentUserService : ICurrentUserService, ICurrentUserAccessor
    {
        public UserSession? User { get; private set; }

        public bool IsAuthenticated => User != null;

        public Guid? UserId => User?.UserId;

        public string? UserName => User?.UserName;

        public string? RoleName => User?.RoleName;

        public Guid? BranchId => User?.BranchId;

        public bool IsAdmin => User?.IsAdmin ?? false;

        public void SetUser(UserSession user)
        {
            User = user;
        }

        public void Clear()
        {
            User = null;
        }
    }
}
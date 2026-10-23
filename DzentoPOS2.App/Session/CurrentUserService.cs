using DzentoPOS2.Shared.Contracts.DTOs.Identity.Auth;

namespace DzentoPOS2.App.Session
{
    public class CurrentUserService : ICurrentUserService
    {
        public UserSession? User { get; private set; }

        public bool IsAuthenticated => User != null;

        public Guid? UserId => User?.UserId;

        public string? UserName => User?.UserName;

        public string? RoleName => User?.RoleName;

        public Guid? BranchId => User?.BranchId;

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
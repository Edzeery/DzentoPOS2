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
            DzentoPOS2.Core.Services.CurrentUserContext.UserId = user.UserId;
            UserChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Clear()
        {
            User = null;
            DzentoPOS2.Core.Services.CurrentUserContext.UserId = null;
            UserChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler? UserChanged;
    }
}
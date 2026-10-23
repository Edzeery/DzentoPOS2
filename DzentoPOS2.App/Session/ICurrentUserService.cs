using DzentoPOS2.Shared.Contracts.DTOs.Identity.Auth;

namespace DzentoPOS2.App.Session
{
    public interface ICurrentUserService
    {
        bool IsAuthenticated { get; }

        UserSession? User { get; }

        Guid? UserId { get; }

        string? UserName { get; }

        string? RoleName { get; }

        Guid? BranchId { get; }

        void SetUser(UserSession user);

        void Clear();
    }
}
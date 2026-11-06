using DzentoPOS2.Shared.Contracts.DTOs.Identity.Auth; 
using DzentoPOS2.Shared.Results;

namespace DzentoPOS2.App.Services.Identity.Interfaces
{
    public interface IAuthService
    {
        Task<Result<LoginResponseDto>> LoginAsync(
            LoginDto dto);

        Task<Result> LogoutAsync();

        Task<Result<string>> ChangePasswordAsync(
            ChangePasswordDto dto);

        /// <summary>
        /// Generates a new recovery key for the given user, stores only its
        /// hash, and returns the plain-text key (shown once to the caller).
        /// </summary>
        Task<Result<string>> GenerateRecoveryKeyAsync(Guid userId);

        /// <summary>
        /// Resets the user's password using a recovery key. On success the
        /// password is replaced, MustChangePassword is cleared, a NEW recovery
        /// key is generated and returned (shown once), and the old key is
        /// invalidated.
        /// </summary>
        Task<Result<string>> ResetPasswordWithRecoveryKeyAsync(
            string userName,
            string recoveryKey,
            string newPassword);
    }
}
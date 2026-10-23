using DzentoPOS2.Shared.Contracts.DTOs.Identity.Auth; 
using DzentoPOS2.Shared.Results;

namespace DzentoPOS2.App.Services.Identity.Interfaces
{
    public interface IAuthService
    {
        Task<Result<LoginResponseDto>> LoginAsync(
            LoginDto dto);

        Task<Result> LogoutAsync();

        Task<Result> ChangePasswordAsync(
            ChangePasswordDto dto);
    }
}
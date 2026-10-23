using DzentoPOS2.Shared.Contracts.DTOs.Identity.Users;
using DzentoPOS2.Core.Common;
using DzentoPOS2.Shared.Contracts.Requests;
using DzentoPOS2.Shared.Contracts.Responses;
using DzentoPOS2.Shared.Results;

namespace DzentoPOS2.App.Services.Identity.Interfaces
{
    public interface IUserService
    {
        Task<Result<UserDetailsDto>> GetByIdAsync(
            Guid id);

        Task<Result<PagedResponse<UserDto>>> GetPagedAsync(
            PagedRequest request);

        Task<Result> CreateAsync(
            CreateUserDto dto);

        Task<Result> UpdateAsync(
            UpdateUserDto dto);

        Task<Result> DeleteAsync(
            Guid id);
    }
}
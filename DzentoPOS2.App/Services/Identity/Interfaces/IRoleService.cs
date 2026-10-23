
using DzentoPOS2.Shared.Contracts.DTOs.Identity.Roles; 
using DzentoPOS2.Shared.Contracts.Requests;
using DzentoPOS2.Shared.Contracts.Responses;
using DzentoPOS2.Shared.Results;


namespace DzentoPOS2.App.Services.Identity.Interfaces
{
    public interface IRoleService
    {
        Task<Result<RoleDetailsDto>>
            GetByIdAsync(Guid id);

        Task<Result<PagedResponse<RoleDto>>>
            GetPagedAsync(
                PagedRequest request);

        Task<Result> CreateAsync(
            CreateRoleDto dto);

        Task<Result> UpdateAsync(
            UpdateRoleDto dto);

        Task<Result> DeleteAsync(
            Guid id);
    }
}
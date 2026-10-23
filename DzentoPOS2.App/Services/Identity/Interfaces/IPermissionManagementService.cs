using DzentoPOS2.Shared.Contracts.DTOs.Identity.Permissions;
using DzentoPOS2.Shared.Results;
namespace DzentoPOS2.App.Services.Identity.Interfaces
{
    public interface IPermissionManagementService
    {
        Task<Result<List<PermissionDto>>>
            GetAllAsync();
    }
}
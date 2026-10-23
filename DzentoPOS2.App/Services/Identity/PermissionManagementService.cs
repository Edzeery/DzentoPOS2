using AutoMapper;
using DzentoPOS2.App.Services.Identity.Interfaces;
using DzentoPOS2.Data.Repositories.Identity.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Identity.Permissions;
using DzentoPOS2.Shared.Results;

namespace DzentoPOS2.App.Services.Identity
{
    public class PermissionManagementService
        : IPermissionManagementService
    {
        private readonly IPermissionRepository
            _permissionRepository;

        private readonly IMapper _mapper;

        public PermissionManagementService(
            IPermissionRepository permissionRepository,
            IMapper mapper)
        {
            _permissionRepository =
                permissionRepository;

            _mapper = mapper;
        }

        public async Task<
            Result<List<PermissionDto>>>
            GetAllAsync()
        {
            var permissions =
                await _permissionRepository
                    .GetAllAsync();

            var result = _mapper
                .Map<List<PermissionDto>>(
                    permissions.OrderBy(x => x.Module)
                        .ThenBy(x => x.Name))
                .ToList();

            return Result<List<PermissionDto>>
                .Succeeded(result);
        }
    }
}
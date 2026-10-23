using AutoMapper;
using DzentoPOS2.Shared.Contracts.DTOs.Identity.Roles;
using DzentoPOS2.App.Services.Identity.Interfaces;
using DzentoPOS2.Core.Entities.Identity;
using DzentoPOS2.Data.Repositories.Common.Interfaces;
using DzentoPOS2.Data.Repositories.Identity.Interfaces;
using DzentoPOS2.Shared.Results;
using DzentoPOS2.Shared.Contracts.Responses;
using DzentoPOS2.Shared.Contracts.Requests; 

namespace DzentoPOS2.App.Services.Identity
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        private readonly IPermissionRepository
            _permissionRepository;

        private readonly IRolePermissionRepository
            _rolePermissionRepository;

        private readonly IUnitOfWork _unitOfWork;

        private readonly IMapper _mapper;

        public RoleService(
            IRoleRepository roleRepository,
            IPermissionRepository permissionRepository,
            IRolePermissionRepository rolePermissionRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _roleRepository = roleRepository;

            _permissionRepository = permissionRepository;

            _rolePermissionRepository =
                rolePermissionRepository;

            _unitOfWork = unitOfWork;

            _mapper = mapper;
        }

        public async Task<Result> CreateAsync(
            CreateRoleDto dto)
        {
            var exists =
                await _roleRepository.ExistsAsync(x =>
                    x.Name == dto.Name);

            if (exists)
            {
                return Result.Failed(
                    "الدور موجود مسبقاً");
            }

            var role = new Role
            {
                Name = dto.Name,
                Description = dto.Description
            };

            await _roleRepository.AddAsync(role);

            foreach (var permissionId in dto.PermissionIds)
            {
                var permission =
                    await _permissionRepository
                        .GetByIdAsync(permissionId);

                if (permission == null)
                    continue;

                await _rolePermissionRepository
                    .AddAsync(new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permissionId
                    });
            }

            await _unitOfWork.SaveChangesAsync();

            return Result.Succeeded(
                "تم إنشاء الدور بنجاح");
        }

        public async Task<Result> UpdateAsync(
            UpdateRoleDto dto)
        {
            var role =
                await _roleRepository
                    .GetWithPermissionsAsync(dto.Id);

            if (role == null)
            {
                return Result.Failed(
                    "الدور غير موجود");
            }

            var duplicate =
                await _roleRepository.ExistsAsync(x =>
                    x.Name == dto.Name &&
                    x.Id != dto.Id);

            if (duplicate)
            {
                return Result.Failed(
                    "اسم الدور مستخدم مسبقاً");
            }

            role.Name = dto.Name;

            role.Description = dto.Description;

            _roleRepository.Update(role);

            var currentPermissions =
                role.RolePermissions
                    .Select(x => x.PermissionId)
                    .ToList();

            var permissionsToRemove =
                currentPermissions
                    .Except(dto.PermissionIds)
                    .ToList();

            var permissionsToAdd =
                dto.PermissionIds
                    .Except(currentPermissions)
                    .ToList();

            foreach (var permissionId
                     in permissionsToRemove)
            {
                await _rolePermissionRepository
                    .RemoveAsync(
                        role.Id,
                        permissionId);
            }

            foreach (var permissionId
                     in permissionsToAdd)
            {
                await _rolePermissionRepository
                    .AddAsync(new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permissionId
                    });
            }

            await _unitOfWork.SaveChangesAsync();

            return Result.Succeeded(
                "تم تحديث الدور بنجاح");
        }

        public async Task<Result> DeleteAsync(
            Guid id)
        {
            var role =
                await _roleRepository
                    .GetWithPermissionsAsync(id);

            if (role == null)
            {
                return Result.Failed(
                    "الدور غير موجود");
            }

            if (role.Name == "SuperAdmin")
            {
                return Result.Failed(
                    "لا يمكن حذف هذا الدور");
            }

            _roleRepository.Remove(role);

            await _unitOfWork.SaveChangesAsync();

            return Result.Succeeded(
                "تم حذف الدور بنجاح");
        }

        public async Task<Result<RoleDetailsDto>>
            GetByIdAsync(Guid id)
        {
            var role =
                await _roleRepository
                    .GetWithPermissionsAsync(id);

            if (role == null)
            {
                return Result<RoleDetailsDto>
                    .Failed("الدور غير موجود");
            }

            var dto = _mapper.Map<RoleDetailsDto>(role);

            return Result<RoleDetailsDto>
                .Succeeded(dto);
        }

        public async Task<
            Result<PagedResponse<RoleDto>>>
            GetPagedAsync(
                PagedRequest request)
        {
            var roles =
                await _roleRepository.GetPagedAsync(
                    request.PageNumber,
                    request.PageSize,
                    request.Search);

            var totalCount =
                await _roleRepository.CountAsync(
                    request.Search);

            var items = _mapper.Map<List<RoleDto>>(roles);

            var response =
                new PagedResponse<RoleDto>
                {
                    Items = items,

                    TotalCount = totalCount,

                    PageNumber = request.PageNumber,

                    PageSize = request.PageSize
                };

            return Result<PagedResponse<RoleDto>>
                .Succeeded(response);
        }
    }
}
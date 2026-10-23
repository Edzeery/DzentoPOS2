using AutoMapper;
using DzentoPOS2.Core.Entities.Accounting;
using DzentoPOS2.Core.Entities.Businesses;
using DzentoPOS2.Core.Entities.Identity;
using DzentoPOS2.Core.Entities.Inventory;
using DzentoPOS2.Shared.Contracts.DTOs.Accounting;
using DzentoPOS2.Shared.Contracts.DTOs.Identity.Permissions;
using DzentoPOS2.Shared.Contracts.DTOs.Identity.Roles;
using DzentoPOS2.Shared.Contracts.DTOs.Identity.Users;
using DzentoPOS2.Shared.Contracts.DTOs.Inventory;

namespace DzentoPOS2.App.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // —————————————————————————————
            // User → UserDto
            // —————————————————————————————
            CreateMap<User, UserDto>()
                .ForMember(d => d.Email, o => o.MapFrom(s => s.Email!.Value))
                .ForMember(d => d.PhoneNumber, o => o.MapFrom(s => s.PhoneNumber!.Value))
                .ForMember(d => d.BranchName, o => o.MapFrom(s => s.Branch!.Name));

            // —————————————————————————————
            // User → UserDetailsDto (extends UserDto)
            // —————————————————————————————
            CreateMap<User, UserDetailsDto>()
                .IncludeBase<User, UserDto>()
                .ForMember(d => d.RoleIds, o => o.MapFrom(s =>
                    s.UserRoles.Select(ur => ur.RoleId)))
                .ForMember(d => d.Roles, o => o.MapFrom(s =>
                    s.UserRoles.Select(ur => ur.Role.Name)));

            CreateMap<CreateUserDto, User>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.PasswordHash, o => o.Ignore())
                .ForMember(d => d.Email, o => o.Ignore())
                .ForMember(d => d.PhoneNumber, o => o.Ignore())
                .ForMember(d => d.UserRoles, o => o.Ignore())
                .ForMember(d => d.Branch, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.UpdatedAt, o => o.Ignore())
                .ForMember(d => d.CreatedBy, o => o.Ignore())
                .ForMember(d => d.UpdatedBy, o => o.Ignore())
                .ForMember(d => d.IsDeleted, o => o.Ignore());

            CreateMap<UpdateUserDto, User>()
                .ForMember(d => d.PasswordHash, o => o.Ignore())
                .ForMember(d => d.Email, o => o.Ignore())
                .ForMember(d => d.PhoneNumber, o => o.Ignore())
                .ForMember(d => d.UserRoles, o => o.Ignore())
                .ForMember(d => d.Branch, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.UpdatedAt, o => o.Ignore())
                .ForMember(d => d.CreatedBy, o => o.Ignore())
                .ForMember(d => d.UpdatedBy, o => o.Ignore())
                .ForMember(d => d.IsDeleted, o => o.Ignore());

            // —————————————————————————————
            // Role → RoleDto
            // —————————————————————————————
            CreateMap<Role, RoleDto>()
                .ForMember(d => d.PermissionsCount, o => o.MapFrom(s =>
                    s.RolePermissions.Count));

            CreateMap<Role, RoleDetailsDto>()
                .ForMember(d => d.PermissionIds, o => o.MapFrom(s =>
                    s.RolePermissions.Select(rp => rp.PermissionId)))
                .ForMember(d => d.Permissions, o => o.MapFrom(s =>
                    s.RolePermissions.Select(rp => rp.Permission.Name)));

            CreateMap<CreateRoleDto, Role>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.UserRoles, o => o.Ignore())
                .ForMember(d => d.RolePermissions, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.UpdatedAt, o => o.Ignore())
                .ForMember(d => d.CreatedBy, o => o.Ignore())
                .ForMember(d => d.UpdatedBy, o => o.Ignore())
                .ForMember(d => d.IsDeleted, o => o.Ignore());

            CreateMap<UpdateRoleDto, Role>()
                .ForMember(d => d.UserRoles, o => o.Ignore())
                .ForMember(d => d.RolePermissions, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.UpdatedAt, o => o.Ignore())
                .ForMember(d => d.CreatedBy, o => o.Ignore())
                .ForMember(d => d.UpdatedBy, o => o.Ignore())
                .ForMember(d => d.IsDeleted, o => o.Ignore());

            CreateMap<Permission, PermissionDto>();

            // —————————————————————————————
            // Product → ProductCategoryDto
            // —————————————————————————————
            CreateMap<ProductCategory, ProductCategoryDto>();

            CreateMap<ProductUnit, ProductUnitDto>();

            // —————————————————————————————
            // ExpenseCategory → ExpenseCategoryDto
            // —————————————————————————————
            CreateMap<ExpenseCategory, ExpenseCategoryDto>();

            CreateMap<Warehouse, WarehouseDto>();
        }
    }
}

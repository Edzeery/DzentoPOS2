using AutoMapper;

using DzentoPOS2.App.Authentication;
using DzentoPOS2.Shared.Contracts.DTOs.Identity.Users;
using DzentoPOS2.App.Services.Identity.Interfaces;

using DzentoPOS2.Core.Common;
using DzentoPOS2.Core.Entities.Identity;
using DzentoPOS2.Core.ValueObjects.Identity;

using DzentoPOS2.Data.Repositories.Common.Interfaces;
using DzentoPOS2.Data.Repositories.Identity.Interfaces;
using DzentoPOS2.Shared.Results;
using DzentoPOS2.Shared.Contracts.Responses;
using DzentoPOS2.Shared.Contracts.Requests;
using DzentoPOS2.Localization.Resources;

namespace DzentoPOS2.App.Services.Identity
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        private readonly IUserRoleRepository _userRoleRepository;

        private readonly IUnitOfWork _unitOfWork;

        private readonly IPasswordHasher _passwordHasher;

        private readonly IMapper _mapper;

        public UserService(
            IUserRepository userRepository,
            IUserRoleRepository userRoleRepository,
            IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            IMapper mapper)
        {
            _userRepository = userRepository;

            _userRoleRepository = userRoleRepository;

            _unitOfWork = unitOfWork;

            _passwordHasher = passwordHasher;

            _mapper = mapper;
        }

        public async Task<Result> CreateAsync(
            CreateUserDto dto)
        {
            var existingUser =
                await _userRepository
                    .GetByUserNameAsync(dto.UserName);

            if (existingUser != null)
            {
                return Result.Failed(AppStrings.Usernamealreadyexists);
            }

            var user = new User
            {
                FullName = dto.FullName,

                UserName = dto.UserName,

                PasswordHash =
                    _passwordHasher.Hash(dto.Password),

                IsActive = dto.IsActive,

                BranchId = dto.BranchId
            };

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                user.Email =
                    new Email(dto.Email);
            }

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
            {
                user.PhoneNumber =
                    new PhoneNumber(dto.PhoneNumber);
            }

            await _userRepository.AddAsync(user);

            await _unitOfWork.SaveChangesAsync();

            foreach (var roleId in dto.RoleIds)
            {
                await _userRoleRepository.AddAsync(
                    new UserRole
                    {
                        UserId = user.Id,
                        RoleId = roleId
                    });
            }

            await _unitOfWork.SaveChangesAsync();

            return Result.Succeeded(AppStrings.Usercreatedsuccessfully);
        }

        public async Task<Result> UpdateAsync(
            UpdateUserDto dto)
        {
            var user =
                await _userRepository
                    .GetWithRolesAsync(dto.Id);

            if (user == null)
            {
                return Result.Failed(
                    AppStrings.Usernotfound);
            }

            user.FullName = dto.FullName;

            user.UserName = dto.UserName;

            user.IsActive = dto.IsActive;

            user.BranchId = dto.BranchId;

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                user.Email =
                    new Email(dto.Email);
            }

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
            {
                user.PhoneNumber =
                    new PhoneNumber(dto.PhoneNumber);
            }

            _userRepository.Update(user);

            foreach (var userRole in user.UserRoles)
            {
                await _userRoleRepository.RemoveAsync(
                    user.Id,
                    userRole.RoleId);
            }

            foreach (var roleId in dto.RoleIds)
            {
                await _userRoleRepository.AddAsync(
                    new UserRole
                    {
                        UserId = user.Id,
                        RoleId = roleId
                    });
            }

            await _unitOfWork.SaveChangesAsync();

            return Result.Succeeded(AppStrings.Usermodifiedsuccessfully);
        }

        public async Task<Result> DeleteAsync(
            Guid id)
        {
            var user =
                await _userRepository
                    .GetByIdAsync(id);

            if (user == null)
            {
                return Result.Failed(
                    AppStrings.Usernotfound);
            }

            _userRepository.Remove(user);

            await _unitOfWork.SaveChangesAsync();

            return Result.Succeeded(AppStrings.UserDeleted);
        }

        public async Task<Result<UserDetailsDto>>
            GetByIdAsync(Guid id)
        {
            var user =
                await _userRepository
                    .GetWithRolesAsync(id);

            if (user == null)
            {
                return Result<UserDetailsDto>
                    .Failed(AppStrings.Usernotfound);
            }

            var dto = _mapper.Map<UserDetailsDto>(user);

            return Result<UserDetailsDto>
                .Succeeded(dto);
        }

        public async Task<
            Result<PagedResponse<UserDto>>>
            GetPagedAsync(
                PagedRequest request)
        {
            var users =
                await _userRepository.GetPagedAsync(
                    request.PageNumber,
                    request.PageSize,
                    request.Search);

            var totalCount =
                await _userRepository.CountAsync(
                    request.Search);

            var items = _mapper.Map<List<UserDto>>(users);

            var response =
                new PagedResponse<UserDto>
                {
                    Items = items,

                    TotalCount = totalCount,

                    PageNumber = request.PageNumber,

                    PageSize = request.PageSize
                };

            return Result<PagedResponse<UserDto>>
                .Succeeded(response);
        }
    }
}
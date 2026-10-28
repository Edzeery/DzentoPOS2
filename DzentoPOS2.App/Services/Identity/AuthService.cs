using DzentoPOS2.Shared.Contracts.DTOs.Identity.Auth;
using DzentoPOS2.App.Services.Identity.Interfaces;
using DzentoPOS2.App.Session;
using DzentoPOS2.App.Authorization;
using DzentoPOS2.Data.Repositories.Common.Interfaces;
using DzentoPOS2.Data.Repositories.Identity.Interfaces;
using DzentoPOS2.Shared.Results;
using DzentoPOS2.Shared.Helpers;
using DzentoPOS2.Localization.Resources;

namespace DzentoPOS2.App.Services.Identity
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;

        private readonly IUserRoleRepository _userRoleRepository;

        private readonly IRoleRepository _roleRepository;

        private readonly IUnitOfWork _unitOfWork;

        private readonly ICurrentUserService _currentUser;

        public AuthService(
            IUserRepository userRepository,
            IUserRoleRepository userRoleRepository,
            IRoleRepository roleRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser)
        {
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _roleRepository = roleRepository;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Result<LoginResponseDto>> LoginAsync(
            LoginDto dto)
        {
            try
            {
                var user = await _userRepository
                    .GetByUserNameAsync(dto.UserName);

                if (user is null)
                {
                    return Result<LoginResponseDto>
                        .Failed(AppStrings.Usernotfound);
                }

                if (!user.IsActive)
                {
                    return Result<LoginResponseDto>
                        .Failed(AppStrings.AccountDeactivated);
                }

                // مؤقتًا إلى حين إضافة Hashing
                bool isPasswordValid = PasswordHelper.Verify(dto.Password, user.PasswordHash);

                // Legacy accounts (created before password hashing was introduced)
                // stored the password in clear text, so BCrypt.Verify always fails
                // for them. Accept the match once and upgrade the stored value to a
                // hash so the account keeps working from then on.
                if (!isPasswordValid && !PasswordHelper.IsHashed(user.PasswordHash))
                {
                    if (string.Equals(user.PasswordHash, dto.Password, StringComparison.Ordinal))
                    {
                        user.PasswordHash = PasswordHelper.Hash(dto.Password);
                        _userRepository.Update(user);
                        await _unitOfWork.SaveChangesAsync();
                        isPasswordValid = true;
                    }
                }

                if (!isPasswordValid)
                {
                    return Result<LoginResponseDto>
                        .Failed(AppStrings.PasswordIncorrect);
                }

                var userRole = await _userRoleRepository
                    .GetByUserIdAsync(user.Id);

                string roleName = string.Empty;

                bool isAdmin = false;

                if (userRole is not null)
                {
                    var role = await _roleRepository
                        .GetByIdAsync(userRole.RoleId);

                    if (role is not null)
                    {
                        roleName = role.Name;

                        isAdmin = role.Name
                            .Equals(
                                "Admin",
                                StringComparison.OrdinalIgnoreCase);
                    }
                }

               var response = new LoginResponseDto
               {
                   UserId = user.Id,
                   UserName = user.UserName,
                   FullName = user.FullName,
                   RoleName = roleName,
                   BranchId = user.BranchId,
                   IsAdmin = isAdmin,
                   LoginTime = DateTime.Now
               };

                _currentUser.SetUser(new UserSession
                {
                    UserId = response.UserId,
                    UserName = response.UserName,
                    FullName = response.FullName,
                    RoleName = response.RoleName,
                    BranchId = response.BranchId,
                    IsAdmin = response.IsAdmin
                });

                try
                {
                    var permissionService = new PermissionService(_currentUser, _userRepository);
                    await permissionService.RefreshCacheAsync(response.UserId);
                }
                catch { }

                return Result<LoginResponseDto>
                    .Succeeded(
                        response, AppStrings.LoginSuccess);
            }
            catch (Exception ex)
            {
                return Result<LoginResponseDto>
                    .Failed(ex.Message);
            }
        }

        public async Task<Result> LogoutAsync()
        {
            try
            {
                _currentUser.Clear();
                await Task.CompletedTask;
                return Result.Succeeded(AppStrings.Logout);
            }
            catch (Exception ex)
            {
                return Result.Failed(ex.Message);
            }
        }

        public async Task<Result> ChangePasswordAsync(
            ChangePasswordDto dto)
        {
            try
            {
                var user = await _userRepository
                    .GetByIdAsync(dto.UserId);

                if (user is null)
                {
                    return Result
                        .Failed(AppStrings.Usernotfound);
                }
                bool isPasswordValid = PasswordHelper.Verify(dto.CurrentPassword, user.PasswordHash);

                if (!isPasswordValid)
                {
                    return Result<LoginResponseDto>
                        .Failed(AppStrings.InvalidCurrentPassword);
                }
                 

                user.PasswordHash = PasswordHelper.Hash(dto.NewPassword);

                _userRepository.Update(user);

                await _unitOfWork.SaveChangesAsync();

                return Result.Succeeded(AppStrings.PasswordChangedSuccessfully);
            }
            catch (Exception ex)
            {
                return Result.Failed(ex.Message);
            }
        }
    }
}
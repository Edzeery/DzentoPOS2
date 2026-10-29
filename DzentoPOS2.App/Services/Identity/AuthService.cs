using DzentoPOS2.Shared.Contracts.DTOs.Identity.Auth;
using DzentoPOS2.App.Services.Identity.Interfaces;
using DzentoPOS2.App.Session;
using DzentoPOS2.App.Authorization;
using DzentoPOS2.Data.Repositories.Common.Interfaces;
using DzentoPOS2.Data.Repositories.Identity.Interfaces;
using DzentoPOS2.Shared.Results;
using DzentoPOS2.Shared.Helpers;
using DzentoPOS2.Localization.Resources;
using Microsoft.Extensions.Logging;

namespace DzentoPOS2.App.Services.Identity
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;

        private readonly IUserRoleRepository _userRoleRepository;

        private readonly IRoleRepository _roleRepository;

        private readonly IUnitOfWork _unitOfWork;

        private readonly ICurrentUserService _currentUser;

        private readonly IPermissionService _permissionService;

        private readonly IAuditService _auditService;

        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IUserRoleRepository userRoleRepository,
            IRoleRepository roleRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IPermissionService permissionService,
            IAuditService auditService,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _roleRepository = roleRepository;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _permissionService = permissionService;
            _auditService = auditService;
            _logger = logger;
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
                                StringComparison.OrdinalIgnoreCase)
                            || role.Name.Equals(
                                "SuperAdmin",
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
                    MustChangePassword = user.MustChangePassword,
                    LoginTime = DateTime.Now
                };

                _currentUser.SetUser(new UserSession
                {
                    UserId = response.UserId,
                    UserName = response.UserName,
                    FullName = response.FullName,
                    RoleName = response.RoleName,
                    BranchId = response.BranchId,
                    IsAdmin = response.IsAdmin,
                    MustChangePassword = response.MustChangePassword
                });

                // The cache is an optimization: a failure here must not block a
                // valid login, but it must be visible in the log.
                try
                {
                    await _permissionService.RefreshCacheAsync(response.UserId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Could not refresh the permission cache for {UserName}.",
                        response.UserName);
                }

                await _auditService.LogAsync(
                    "Login",
                    "User",
                    newValues: response.UserName,
                    userId: response.UserId);

                if (string.Equals(
                        roleName,
                        "SuperAdmin",
                        StringComparison.OrdinalIgnoreCase))
                {
                    // The full-access role bypasses the permission table; keep
                    // one auditable record that this session could do anything.
                    await _auditService.LogAsync(
                        "SuperAdminBypass",
                        "Role",
                        newValues: roleName,
                        userId: response.UserId);
                }

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
                var userId = _currentUser.UserId;
                var userName = _currentUser.UserName;

                _currentUser.Clear();
                await Task.CompletedTask;

                await _auditService.LogAsync(
                    "Logout",
                    "User",
                    newValues: userName ?? string.Empty,
                    userId: userId);

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
                // Read the tracked instance: the same page scope may already be
                // tracking this user (the login call did), and attaching a
                // detached copy with the same key would throw.
                var user = await _userRepository
                    .GetWithRolesAsync(dto.UserId);

                if (user is null)
                {
                    return Result
                        .Failed(AppStrings.Usernotfound);
                }
                bool isPasswordValid = PasswordHelper.Verify(dto.CurrentPassword, user.PasswordHash);

                if (!isPasswordValid)
                {
                    return Result
                        .Failed(AppStrings.InvalidCurrentPassword);
                }

                user.PasswordHash = PasswordHelper.Hash(dto.NewPassword);

                // The account no longer uses a seeded/assigned password.
                user.MustChangePassword = false;

                _userRepository.Update(user);

                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogAsync(
                    "ChangePassword",
                    "User",
                    newValues: user.UserName,
                    userId: user.Id);

                return Result.Succeeded(AppStrings.PasswordChangedSuccessfully);
            }
            catch (Exception ex)
            {
                return Result.Failed(ex.Message);
            }
        }
    }
}
using DzentoPOS2.Core.Entities.Identity;
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

        public async Task<Result<string>> ChangePasswordAsync(
            ChangePasswordDto dto)
        {
            try
            {
                var user = await _userRepository
                    .GetWithRolesAsync(dto.UserId);

                if (user is null)
                {
                    return Result<string>
                        .Failed(AppStrings.Usernotfound);
                }
                bool isPasswordValid = PasswordHelper.Verify(dto.CurrentPassword, user.PasswordHash);

                if (!isPasswordValid)
                {
                    return Result<string>
                        .Failed(AppStrings.InvalidCurrentPassword);
                }

                user.PasswordHash = PasswordHelper.Hash(dto.NewPassword);
                user.MustChangePassword = false;

                var keyResult = await GenerateRecoveryKeyInternalAsync(user);

                _userRepository.Update(user);
                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogAsync(
                    "ChangePassword",
                    "User",
                    newValues: user.UserName,
                    userId: user.Id);

                if (!keyResult.Success)
                {
                    _logger.LogWarning(
                        "Could not generate recovery key: {Error}",
                        keyResult.Message);
                    return Result<string>.Succeeded(string.Empty);
                }

                return Result<string>.Succeeded(keyResult.Data!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ChangePasswordAsync failed.");
                return Result<string>.Failed(AppStrings.ErrorMsg);
            }
        }

        public async Task<Result<string>> GenerateRecoveryKeyAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository
                    .GetWithRolesAsync(userId);

                if (user is null)
                {
                    return Result<string>.Failed(AppStrings.Usernotfound);
                }

                var keyResult = await GenerateRecoveryKeyInternalAsync(user);
                if (!keyResult.Success)
                {
                    return Result<string>.Failed(keyResult.Message ?? AppStrings.ErrorMsg);
                }

                _userRepository.Update(user);
                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogAsync(
                    "RecoveryKeyGenerated",
                    "User",
                    newValues: user.UserName,
                    userId: user.Id);

                return Result<string>.Succeeded(keyResult.Data!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate recovery key.");
                return Result<string>.Failed(AppStrings.ErrorMsg);
            }
        }

        private Task<Result<string>> GenerateRecoveryKeyInternalAsync(User user)
        {
            try
            {
                var bytes = new byte[16];
                using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
                {
                    rng.GetBytes(bytes);
                }

                var hex = Convert.ToHexString(bytes);
                var groups = new List<string>();
                for (int i = 0; i < hex.Length; i += 4)
                {
                    groups.Add(hex.Substring(i, 4));
                }
                var plainKey = string.Join("-", groups);

                user.RecoveryKeyHash = PasswordHelper.Hash(plainKey);

                return Task.FromResult(Result<string>.Succeeded(plainKey));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate recovery key.");
                return Task.FromResult(Result<string>.Failed(AppStrings.ErrorMsg));
            }
        }

        public async Task<Result<string>> ResetPasswordWithRecoveryKeyAsync(
            string userName,
            string recoveryKey,
            string newPassword)
        {
            try
            {
                var user = await _userRepository
                    .GetByUserNameAsync(userName);

                if (user is null || string.IsNullOrWhiteSpace(user.RecoveryKeyHash))
                {
                    await RecordFailedAttemptAsync(user, userName);
                    await _auditService.LogAsync(
                        "RecoveryKeyFailed",
                        "User",
                        newValues: userName,
                        userId: null);
                    return Result<string>.Failed(AppStrings.InvalidRecoveryKey);
                }

                if (user.RecoveryLockedUntilUtc.HasValue &&
                    user.RecoveryLockedUntilUtc.Value > DateTime.UtcNow)
                {
                    return Result<string>.Failed(AppStrings.RecoveryKeyLockedOut);
                }

                if (user.RecoveryLockedUntilUtc.HasValue &&
                    user.RecoveryLockedUntilUtc.Value <= DateTime.UtcNow)
                {
                    user.RecoveryFailedCount = 0;
                    user.RecoveryLockedUntilUtc = null;
                }

                if (!PasswordHelper.Verify(recoveryKey, user.RecoveryKeyHash))
                {
                    await RecordFailedAttemptAsync(user, userName);
                    await _auditService.LogAsync(
                        "RecoveryKeyFailed",
                        "User",
                        newValues: userName,
                        userId: user.Id);
                    return Result<string>.Failed(AppStrings.InvalidRecoveryKey);
                }

                if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
                {
                    return Result<string>.Failed(AppStrings.PasswordTooShort);
                }

                if (PasswordHelper.Verify(newPassword, user.PasswordHash))
                {
                    return Result<string>.Failed(AppStrings.PasswordMustDiffer);
                }

                user.PasswordHash = PasswordHelper.Hash(newPassword);
                user.MustChangePassword = false;

                var keyResult = await GenerateRecoveryKeyInternalAsync(user);
                if (!keyResult.Success)
                {
                    _logger.LogWarning(
                        "Could not generate new recovery key: {Error}",
                        keyResult.Message);
                }

                user.RecoveryFailedCount = 0;
                user.RecoveryLockedUntilUtc = null;

                _userRepository.Update(user);
                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogAsync(
                    "RecoveryKeySuccess",
                    "User",
                    newValues: userName,
                    userId: user.Id);

                return Result<string>.Succeeded(
                    keyResult.Data ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reset password with recovery key.");
                return Result<string>.Failed(AppStrings.ErrorMsg);
            }
        }

        private async Task RecordFailedAttemptAsync(User? user, string userName)
        {
            if (user is null) return;

            user.RecoveryFailedCount++;
            user.RecoveryLockedUntilUtc = null;
            if (user.RecoveryFailedCount >= 5)
            {
                user.RecoveryLockedUntilUtc = DateTime.UtcNow.AddMinutes(15);
            }

            _userRepository.Update(user);
            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to persist recovery lockout state.");
            }
        }
    }
}
using DzentoPOS2.App.Session;
using DzentoPOS2.Core.Exceptions;
using DzentoPOS2.Localization.Resources;

namespace DzentoPOS2.App.Authorization;

/// <summary>
/// Service-layer permission enforcement. UI checks are an affordance; this guard
/// is the actual boundary: every sensitive service call goes through
/// <see cref="DemandAsync"/> and fails with a coded, localizable domain
/// exception when the signed-in user is not allowed to do the work.
/// </summary>
public interface IAuthorizationGuard
{
    Task<bool> IsAllowedAsync(string permissionCode);

    /// <summary>
    /// Throws <see cref="CodedDomainException"/> with
    /// <see cref="DomainErrorCodes.PermissionDenied"/> when the permission is
    /// missing or nobody is signed in.
    /// </summary>
    Task DemandAsync(string permissionCode);
}

public sealed class AuthorizationGuard : IAuthorizationGuard
{
    private readonly ICurrentUserService _currentUser;
    private readonly IPermissionService _permissionService;
    private readonly IAuditService _auditService;

    public AuthorizationGuard(
        ICurrentUserService currentUser,
        IPermissionService permissionService,
        IAuditService auditService)
    {
        _currentUser = currentUser;
        _permissionService = permissionService;
        _auditService = auditService;
    }

    public async Task<bool> IsAllowedAsync(string permissionCode)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        // Full-access roles bypass the permission table. The bypass is audited
        // once at sign-in (AuthService) instead of on every call, so the audit
        // table does not grow with one row per service operation.
        if (IsSuperAdmin())
            return true;

        if (_currentUser.IsAdmin)
            return true;

        return await _permissionService.HasPermissionAsync(permissionCode);
    }

    public async Task DemandAsync(string permissionCode)
    {
        if (await IsAllowedAsync(permissionCode))
            return;

        await _auditService.LogAsync(
            "PermissionDenied",
            "Permission",
            newValues: permissionCode);

        throw new CodedDomainException(
            DomainErrorCodes.PermissionDenied,
            AppStrings.AccessDenied);
    }

    private bool IsSuperAdmin() =>
        string.Equals(
            _currentUser.RoleName,
            "SuperAdmin",
            StringComparison.OrdinalIgnoreCase);
}
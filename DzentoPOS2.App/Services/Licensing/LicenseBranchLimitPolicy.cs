using DzentoPOS2.App.Services.Systems;
using DzentoPOS2.Core.Exceptions;

namespace DzentoPOS2.App.Services.Licensing;

/// <summary>
/// Real <see cref="IBranchLimitPolicy"/> backed by the active licence plan.
/// It replaces the old always-allow stub so the branch ceiling is enforced.
/// </summary>
public sealed class LicenseBranchLimitPolicy : IBranchLimitPolicy
{
    private readonly ILicenseService _license;

    public LicenseBranchLimitPolicy(ILicenseService license)
    {
        _license = license;
    }

    public async Task<bool> CanCreateBranchAsync(CancellationToken ct = default)
    {
        try
        {
            await _license.EnsureCanCreateBranchAsync(ct);
            return true;
        }
        catch (CodedDomainException)
        {
            return false;
        }
    }

    public async Task<int> MaxBranchesAsync(CancellationToken ct = default) =>
        (await _license.GetLimitsAsync(ct)).MaxBranches;
}

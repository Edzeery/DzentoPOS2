using System.Threading;
using System.Threading.Tasks;

namespace DzentoPOS2.App.Services.Systems;

/// <summary>
/// Branch limit policy (extension point). The real implementation
/// (<c>LicenseBranchLimitPolicy</c>) reads <c>MaxBranches</c> from the active
/// licence plan; consumers depend on this interface instead of the licence
/// service directly.
/// </summary>
public interface IBranchLimitPolicy
{
    Task<bool> CanCreateBranchAsync(CancellationToken ct = default);
    Task<int> MaxBranchesAsync(CancellationToken ct = default);
}

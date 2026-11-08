using System.Threading;
using System.Threading.Tasks;

namespace DzentoPOS2.App.Services.Systems;

/// <summary>
/// Branch limit policy (extension point). Reads MaxBranches from LicensePlan.
/// Not activated yet — stub implementation.
/// </summary>
public interface IBranchLimitPolicy
{
    Task<bool> CanCreateBranchAsync(CancellationToken ct = default);
    Task<int> MaxBranchesAsync(CancellationToken ct = default);
}

public class BranchLimitPolicyStub : IBranchLimitPolicy
{
    public Task<bool> CanCreateBranchAsync(CancellationToken ct = default) => Task.FromResult(true);
    public Task<int> MaxBranchesAsync(CancellationToken ct = default) => Task.FromResult(int.MaxValue);
}

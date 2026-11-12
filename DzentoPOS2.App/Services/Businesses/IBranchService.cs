using DzentoPOS2.Shared.Contracts.DTOs.Businesses;

namespace DzentoPOS2.App.Services.Businesses;

/// <summary>
/// Branch management. A branch owns a default warehouse and a default cashbox;
/// they are created together with the branch in a single transaction so a
/// branch can never exist without a place to store stock and a place to hold
/// money.
/// </summary>
public interface IBranchService
{
    /// <summary>Every non-deleted branch with its warehouse/cashbox counts.</summary>
    Task<List<BranchListItemDto>> GetAllAsync();

    Task<BranchDto?> GetByIdAsync(Guid id);

    /// <summary>
    /// Creates the branch plus its default warehouse and cashbox atomically.
    /// The first branch created becomes the main branch.
    /// </summary>
    Task<BranchDto> CreateAsync(CreateBranchDto dto);

    /// <summary>Updates the descriptive fields (name, address, phone).</summary>
    Task<BranchDto> UpdateAsync(UpdateBranchDto dto);

    /// <summary>
    /// Activates or deactivates a branch. Deactivation never deletes data and
    /// is rejected for the main branch, the last active branch, or a branch
    /// with an open cash session.
    /// </summary>
    Task<BranchDto> SetActiveAsync(Guid id, bool isActive);
}

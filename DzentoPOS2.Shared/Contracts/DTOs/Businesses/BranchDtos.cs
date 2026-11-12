namespace DzentoPOS2.Shared.Contracts.DTOs.Businesses;

/// <summary>
/// A branch as returned to the UI. The value objects (<c>Address</c>,
/// <c>PhoneNumber</c>) are flattened so the presentation layer never has to know
/// about the domain value types.
/// </summary>
public class BranchDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Country { get; set; }

    public string? City { get; set; }

    public string? Street { get; set; }

    public string? PhoneNumber { get; set; }

    public bool IsMainBranch { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// List projection: the branch plus the counts the management screen needs to
/// explain the impact of deactivating it. Never includes deleted children.
/// </summary>
public class BranchListItemDto : BranchDto
{
    public int WarehousesCount { get; set; }

    public int CashboxesCount { get; set; }

    /// <summary>
    /// True while any cashbox of this branch has an open cash session. A branch
    /// in this state must not be deactivated.
    /// </summary>
    public bool HasOpenCashSession { get; set; }
}

public class CreateBranchDto
{
    public string Name { get; set; } = string.Empty;

    public string? Country { get; set; }

    public string? City { get; set; }

    public string? Street { get; set; }

    public string? PhoneNumber { get; set; }

    public bool IsActive { get; set; } = true;
}

public class UpdateBranchDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Country { get; set; }

    public string? City { get; set; }

    public string? Street { get; set; }

    public string? PhoneNumber { get; set; }
}

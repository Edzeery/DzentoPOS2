namespace DzentoPOS2.Shared.Contracts.DTOs.Businesses;

/// <summary>
/// A warehouse of a branch as returned to the management screen.
/// </summary>
public class WarehouseListItemDto
{
    public Guid Id { get; set; }

    public Guid BranchId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Location { get; set; }

    public bool IsActive { get; set; }
}

/// <summary>
/// A cashbox of a branch plus the flag the screen needs to explain why it
/// cannot be deactivated.
/// </summary>
public class CashboxListItemDto
{
    public Guid Id { get; set; }

    public Guid BranchId { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal CurrentBalance { get; set; }

    public bool IsActive { get; set; }

    /// <summary>
    /// True while this cashbox has an open cash session. A cashbox in this
    /// state must not be deactivated.
    /// </summary>
    public bool HasOpenSession { get; set; }
}

public class CreateWarehouseDto
{
    public Guid BranchId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Location { get; set; }

    public bool IsActive { get; set; } = true;
}

public class CreateCashboxDto
{
    public Guid BranchId { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

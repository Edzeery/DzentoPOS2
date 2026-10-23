namespace DzentoPOS2.Shared.Contracts.DTOs.Accounting;

public class ExpenseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid ExpenseCategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string? Notes { get; set; }
}

public class CreateExpenseDto
{
    public string Title { get; set; } = string.Empty;
    public Guid ExpenseCategoryId { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; } = DateTime.Now;
    public string? Notes { get; set; }
}

public class UpdateExpenseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid ExpenseCategoryId { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string? Notes { get; set; }
}

public class ExpenseCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

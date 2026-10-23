using DzentoPOS2.Shared.Contracts.DTOs.Accounting;

namespace DzentoPOS2.App.Services.Accounting;

public interface IExpenseService
{
    Task<List<ExpenseDto>> GetAllAsync();
    Task<ExpenseDto?> GetByIdAsync(Guid id);
    Task<List<ExpenseDto>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<List<ExpenseDto>> GetByCategoryAsync(Guid categoryId);
    Task<ExpenseDto> CreateAsync(CreateExpenseDto dto);
    Task<ExpenseDto> UpdateAsync(UpdateExpenseDto dto);
    Task DeleteAsync(Guid id);
    Task<List<ExpenseCategoryDto>> GetCategoriesAsync();
}

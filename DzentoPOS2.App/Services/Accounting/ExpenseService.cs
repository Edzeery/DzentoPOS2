using AutoMapper;
using DzentoPOS2.App.Services.Accounting;
using DzentoPOS2.App.Session;
using DzentoPOS2.Core.Entities.Accounting;
using DzentoPOS2.Data.Context;
using DzentoPOS2.Data.Repositories.Accounting.Interfaces;
using DzentoPOS2.Data.Repositories.Common.Interfaces;
using DzentoPOS2.Shared.Contracts.DTOs.Accounting;
using Microsoft.EntityFrameworkCore;

namespace DzentoPOS2.App.Services.Accounting;

    public class ExpenseService : IExpenseService
    {
        private readonly IExpenseRepository _expenseRepo;
        private readonly IExpenseCategoryRepository _categoryRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUser;
        private readonly AppDbContext _context;
        private readonly DzentoPOS2.App.Services.Systems.IStoreContextService _storeContext;

    public ExpenseService(
        IExpenseRepository expenseRepo,
        IExpenseCategoryRepository categoryRepo,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUser,
        AppDbContext context,
        DzentoPOS2.App.Services.Systems.IStoreContextService storeContext)
    {
        _expenseRepo = expenseRepo;
        _categoryRepo = categoryRepo;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
        _context = context;
        _storeContext = storeContext;
    }

    public async Task<List<ExpenseDto>> GetAllAsync()
    {
        var expenses = await _expenseRepo.GetAllAsync();
        return await MapWithCategory(expenses);
    }

    public async Task<ExpenseDto?> GetByIdAsync(Guid id)
    {
        var expense = await _expenseRepo.GetByIdAsync(id);
        return expense == null ? null : await MapSingle(expense);
    }

    public async Task<List<ExpenseDto>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        var expenses = await _expenseRepo.GetByDateRangeAsync(from, to);
        return await MapWithCategory(expenses);
    }

    public async Task<List<ExpenseDto>> GetByCategoryAsync(Guid categoryId)
    {
        var expenses = await _expenseRepo.FindAsync(e => e.ExpenseCategoryId == categoryId);
        return await MapWithCategory(expenses);
    }

    public async Task<ExpenseDto> CreateAsync(CreateExpenseDto dto)
    {
        // Wrapped so a failure during the pre-transaction lookups also rolls
        // back and clears the shared change tracker.
        try
        {
            return await CreateInternalAsync(dto);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            _context.ChangeTracker.Clear();
            throw;
        }
    }

    private async Task<ExpenseDto> CreateInternalAsync(CreateExpenseDto dto)
    {
        var branchId = _currentUser.User?.BranchId ?? Guid.Empty;
        if (branchId == Guid.Empty)
            branchId = await _storeContext.GetDefaultBranchIdAsync();

        var cashboxId = await _storeContext.GetDefaultCashboxIdAsync(branchId);
        var session = await _context.CashSessions
            .FirstOrDefaultAsync(x =>
                x.CashboxId == cashboxId &&
                x.Status == Core.Enums.CashSessionStatus.Open &&
                !x.IsDeleted)
            ?? throw new DzentoPOS2.Core.Exceptions.CashSessionRequiredException();

        var expense = new Expense
        {
            Title = dto.Title,
            ExpenseCategoryId = dto.ExpenseCategoryId,
            BranchId = branchId,
            Amount = dto.Amount,
            ExpenseDate = dto.ExpenseDate,
            Notes = dto.Notes
        };

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            await _expenseRepo.AddAsync(expense);

            await _context.CashTransactions.AddAsync(new CashTransaction
            {
                CashboxId = cashboxId,
                CashSessionId = session.Id,
                Amount = dto.Amount,
                TransactionType = Core.Enums.MovementType.Purchase,
                SourceType = "Expense",
                SourceId = expense.Id,
                ReferenceNumber = expense.Id.ToString(),
                Notes = dto.Title,
                TransactionDate = DateTime.UtcNow
            });

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            _context.ChangeTracker.Clear();
            throw;
        }

        return await MapSingle(expense);
    }

    public async Task<ExpenseDto> UpdateAsync(UpdateExpenseDto dto)
    {
        var expense = await _context.Expenses
            .FirstOrDefaultAsync(e => e.Id == dto.Id && !e.IsDeleted);
        if (expense == null)
            throw new KeyNotFoundException($"Expense {dto.Id} not found");

        expense.Title = dto.Title;
        expense.ExpenseCategoryId = dto.ExpenseCategoryId;
        expense.Amount = dto.Amount;
        expense.ExpenseDate = dto.ExpenseDate;
        expense.Notes = dto.Notes;

        _expenseRepo.Update(expense);
        await _unitOfWork.SaveChangesAsync();

        return await MapSingle(expense);
    }

    public async Task DeleteAsync(Guid id)
    {
        var expense = await _context.Expenses
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
        if (expense == null) return;

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            _expenseRepo.Remove(expense);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            _context.ChangeTracker.Clear();
            throw;
        }
    }

    public async Task<List<ExpenseCategoryDto>> GetCategoriesAsync()
    {
        var categories = await _categoryRepo.GetAllAsync();
        return _mapper.Map<List<ExpenseCategoryDto>>(categories);
    }

    private async Task<List<ExpenseDto>> MapWithCategory(IEnumerable<Expense> expenses)
    {
        var result = new List<ExpenseDto>();
        foreach (var e in expenses)
            result.Add(await MapSingle(e));
        return result;
    }

    private async Task<ExpenseDto> MapSingle(Expense expense)
    {
        var categoryName = "";
        var category = await _categoryRepo.GetByIdAsync(expense.ExpenseCategoryId);
        if (category != null)
            categoryName = category.Name;

        return new ExpenseDto
        {
            Id = expense.Id,
            Title = expense.Title,
            ExpenseCategoryId = expense.ExpenseCategoryId,
            CategoryName = categoryName,
            BranchId = expense.BranchId,
            Amount = expense.Amount,
            ExpenseDate = expense.ExpenseDate,
            Notes = expense.Notes
        };
    }
}

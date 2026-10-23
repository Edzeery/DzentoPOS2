using DzentoPOS2.Shared.Contracts.DTOs.Inventory;
using FluentValidation;

namespace DzentoPOS2.App.Validators.Inventory;

public class CreateProductCategoryValidator
    : BaseValidator<CreateProductCategoryDto>
{
    public CreateProductCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");
    }
}

public class UpdateProductCategoryValidator
    : BaseValidator<UpdateProductCategoryDto>
{
    public UpdateProductCategoryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");
    }
}

public class CreateProductUnitValidator
    : BaseValidator<CreateProductUnitDto>
{
    public CreateProductUnitValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Unit name is required.")
            .MaximumLength(50).WithMessage("Unit name must not exceed 50 characters.");

        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("Unit symbol is required.")
            .MaximumLength(10).WithMessage("Unit symbol must not exceed 10 characters.");
    }
}

public class UpdateProductUnitValidator
    : BaseValidator<UpdateProductUnitDto>
{
    public UpdateProductUnitValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Unit name is required.")
            .MaximumLength(50).WithMessage("Unit name must not exceed 50 characters.");

        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("Unit symbol is required.")
            .MaximumLength(10).WithMessage("Unit symbol must not exceed 10 characters.");
    }
}

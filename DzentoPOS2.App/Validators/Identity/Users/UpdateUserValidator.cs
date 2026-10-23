using DzentoPOS2.Shared.Contracts.DTOs.Identity.Users;
using FluentValidation;

namespace DzentoPOS2.App.Validators.Identity.Users
{
    public class UpdateUserValidator
        : BaseValidator<UpdateUserDto>
    {
        public UpdateUserValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();

            RuleFor(x => x.FullName)
                .NotEmpty()
                .MaximumLength(150);

            RuleFor(x => x.UserName)
                .NotEmpty()
                .MaximumLength(100);
        }
    }
}
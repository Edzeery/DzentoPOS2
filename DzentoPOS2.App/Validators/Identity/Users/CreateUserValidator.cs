using DzentoPOS2.Shared.Contracts.DTOs.Identity.Users;
using FluentValidation;

namespace DzentoPOS2.App.Validators.Identity.Users
{
    public class CreateUserValidator
        : BaseValidator<CreateUserDto>
    {
        public CreateUserValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty()
                .MaximumLength(150);

            RuleFor(x => x.UserName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(6);
        }
    }
}
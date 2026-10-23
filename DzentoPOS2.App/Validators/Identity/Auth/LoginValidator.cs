using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Shared.Contracts.DTOs.Identity.Auth;
using FluentValidation;

namespace DzentoPOS2.App.Validators.Identity.Auth
{
    public class LoginValidator
        : BaseValidator<LoginDto>
    {
        public LoginValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty()
                .WithMessage(AppStrings.UserNamRequired);

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage(AppStrings.PasswordRequired);
        }
    }
}
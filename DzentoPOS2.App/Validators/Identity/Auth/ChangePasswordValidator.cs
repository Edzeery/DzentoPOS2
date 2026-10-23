using DzentoPOS2.Localization.Resources;
using DzentoPOS2.Shared.Contracts.DTOs.Identity.Auth;
using FluentValidation;

namespace DzentoPOS2.App.Validators.Identity.Auth
{
    public class ChangePasswordValidator
        : BaseValidator<ChangePasswordDto>
    {
        public ChangePasswordValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty()
                .WithMessage(AppStrings.CurrentPasswordRequired);

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .WithMessage(AppStrings.NewPasswordRequired)
                .MinimumLength(6)
                .WithMessage(AppStrings.PasswordTooShort);

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.NewPassword)
                .WithMessage(AppStrings.PasswordsDoNotMatch);
        }
    }
}
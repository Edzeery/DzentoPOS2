using DzentoPOS2.Shared.Contracts.DTOs.Identity.Roles;
using FluentValidation;

namespace DzentoPOS2.App.Validators.Identity.Roles
{
    public class CreateRoleValidator
        : BaseValidator<CreateRoleDto>
    {
        public CreateRoleValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);
        }
    }
}
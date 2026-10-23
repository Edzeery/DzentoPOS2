using DzentoPOS2.Shared.Contracts.DTOs.Identity.Roles;
using FluentValidation;

namespace DzentoPOS2.App.Validators.Identity.Roles
{
    public class UpdateRoleValidator
        : BaseValidator<UpdateRoleDto>
    {
        public UpdateRoleValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();

            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);
        }
    }
}
using FluentValidation;
using SoberNetwork.Core.DTOs.Groups;

namespace SoberNetwork.Core.Validators.Groups;

public class CreateGroupRequestValidator : AbstractValidator<CreateGroupRequest>
{
    public CreateGroupRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(50)
            .Matches("^[a-z0-9-]+$")
            .WithMessage("Slug may only contain lowercase letters, numbers, and hyphens.");
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.TimeZone).MaximumLength(100);
    }
}

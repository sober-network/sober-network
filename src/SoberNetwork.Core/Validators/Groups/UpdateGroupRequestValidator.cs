using FluentValidation;
using SoberNetwork.Core.DTOs.Groups;

namespace SoberNetwork.Core.Validators.Groups;

public class UpdateGroupRequestValidator : AbstractValidator<UpdateGroupRequest>
{
    public UpdateGroupRequestValidator()
    {
        RuleFor(x => x.Name).MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.TimeZone).MaximumLength(100);
    }
}

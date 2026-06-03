using FluentValidation;
using SoberNetwork.Core.DTOs.Members;

namespace SoberNetwork.Core.Validators.Members;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.DisplayName).MaximumLength(100);
        RuleFor(x => x.FirstName).MaximumLength(50);
        RuleFor(x => x.TimeZone).MaximumLength(100);
    }
}

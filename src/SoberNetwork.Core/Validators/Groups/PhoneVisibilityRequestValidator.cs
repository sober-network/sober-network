using FluentValidation;
using SoberNetwork.Core.DTOs.Groups;

namespace SoberNetwork.Core.Validators.Groups;

public class PhoneVisibilityRequestValidator : AbstractValidator<PhoneVisibilityRequest>
{
    public PhoneVisibilityRequestValidator()
    {
    }
}

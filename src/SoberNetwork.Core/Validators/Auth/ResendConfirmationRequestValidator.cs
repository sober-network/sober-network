using FluentValidation;
using SoberNetwork.Core.DTOs.Auth;

namespace SoberNetwork.Core.Validators.Auth;

public class ResendConfirmationRequestValidator : AbstractValidator<ResendConfirmationRequest>
{
    public ResendConfirmationRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

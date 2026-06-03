using FluentValidation;
using SoberNetwork.Core.DTOs.Auth;

namespace SoberNetwork.Core.Validators.Auth;

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

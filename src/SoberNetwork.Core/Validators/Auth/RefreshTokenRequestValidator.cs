using FluentValidation;
using SoberNetwork.Core.DTOs.Auth;

namespace SoberNetwork.Core.Validators.Auth;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

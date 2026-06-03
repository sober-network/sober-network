using FluentValidation;
using SoberNetwork.Core.DTOs.Members;

namespace SoberNetwork.Core.Validators.Members;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(10);
        RuleFor(x => x.ConfirmNewPassword).NotEmpty();
    }
}

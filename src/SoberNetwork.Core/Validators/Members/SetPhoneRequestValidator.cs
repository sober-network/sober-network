using FluentValidation;
using SoberNetwork.Core.DTOs.Members;

namespace SoberNetwork.Core.Validators.Members;

public class SetPhoneRequestValidator : AbstractValidator<SetPhoneRequest>
{
    public SetPhoneRequestValidator()
    {
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
    }
}

using FluentValidation;
using SoberNetwork.Core.DTOs.Members;

namespace SoberNetwork.Core.Validators.Members;

public class SetSobrietyDateRequestValidator : AbstractValidator<SetSobrietyDateRequest>
{
    public SetSobrietyDateRequestValidator()
    {
        RuleFor(x => x.SobrietyDate).NotEmpty();
    }
}

using FluentValidation;
using SoberNetwork.Core.DTOs.Groups;

namespace SoberNetwork.Core.Validators.Groups;

public class ChangeMemberStatusRequestValidator : AbstractValidator<ChangeMemberStatusRequest>
{
    public ChangeMemberStatusRequestValidator()
    {
        RuleFor(x => x.NewStatus).IsInEnum();
    }
}

using FluentValidation;
using SoberNetwork.Core.DTOs.Members;

namespace SoberNetwork.Core.Validators.Members;

public class DeleteAccountRequestValidator : AbstractValidator<DeleteAccountRequest>
{
    public DeleteAccountRequestValidator()
    {
        RuleFor(x => x.Password).NotEmpty();
    }
}

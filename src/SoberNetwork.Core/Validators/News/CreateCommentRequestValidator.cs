using FluentValidation;
using SoberNetwork.Core.DTOs.News;

namespace SoberNetwork.Core.Validators.News;

public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentRequestValidator()
    {
        RuleFor(x => x.Body).NotEmpty().MaximumLength(2000);
    }
}

using FluentValidation;
using SoberNetwork.Core.DTOs.News;

namespace SoberNetwork.Core.Validators.News;

public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentRequestValidator()
    {
        RuleFor(x => x.Body).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.ImageUrl).MaximumLength(500).When(x => x.ImageUrl != null);
        RuleFor(x => x.LinkUrl).MaximumLength(500).When(x => x.LinkUrl != null);
        RuleFor(x => x.LinkTitle).MaximumLength(200).When(x => x.LinkTitle != null);
    }
}

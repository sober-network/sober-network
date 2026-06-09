using FluentValidation;
using SoberNetwork.Core.DTOs.News;

namespace SoberNetwork.Core.Validators.News;

public class CreatePostRequestValidator : AbstractValidator<CreatePostRequest>
{
    public CreatePostRequestValidator()
    {
        RuleFor(x => x.GroupSlug).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Body).NotEmpty().MaximumLength(5000);
        RuleFor(x => x.ImageUrl).MaximumLength(500).When(x => x.ImageUrl != null);
        RuleFor(x => x.LinkUrl).MaximumLength(500).When(x => x.LinkUrl != null);
        RuleFor(x => x.LinkTitle).MaximumLength(200).When(x => x.LinkTitle != null);
    }
}

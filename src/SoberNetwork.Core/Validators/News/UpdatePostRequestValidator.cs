using FluentValidation;
using SoberNetwork.Core.DTOs.News;

namespace SoberNetwork.Core.Validators.News;

public class UpdatePostRequestValidator : AbstractValidator<UpdatePostRequest>
{
    public UpdatePostRequestValidator()
    {
        RuleFor(x => x.Subject).MaximumLength(200).When(x => x.Subject != null);
        RuleFor(x => x.Body).MaximumLength(5000).When(x => x.Body != null);
        RuleFor(x => x.ImageUrl).MaximumLength(500).When(x => x.ImageUrl != null);
        RuleFor(x => x.LinkUrl).MaximumLength(500).When(x => x.LinkUrl != null);
        RuleFor(x => x.LinkTitle).MaximumLength(200).When(x => x.LinkTitle != null);
    }
}

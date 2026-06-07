using FluentValidation;
using SoberNetwork.Core.DTOs.Common;

namespace SoberNetwork.Core.Validators.Groups;

/// <summary>Validates member-list query parameters at the API boundary.</summary>
public class MembersQueryParamsValidator : AbstractValidator<MembersQueryParams>
{
    /// <summary>Initializes validation rules for <see cref="MembersQueryParams"/>.</summary>
    public MembersQueryParamsValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(500);
        RuleFor(x => x.Search).MaximumLength(100).When(x => x.Search != null);
    }
}

using FluentValidation;
using SoberNetwork.Core.DTOs.Common;

namespace SoberNetwork.Core.Validators.Common;

/// <summary>Validates pagination query parameters at the API boundary.</summary>
public class PaginationQueryValidator : AbstractValidator<PaginationQuery>
{
    public PaginationQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("page must be >= 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("pageSize must be between 1 and 100.");
    }
}

using FluentValidation;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Domain.Enums;

namespace SoberNetwork.Core.Validators.Groups;

/// <summary>Validates requests that assign group service roles.</summary>
public class AssignServiceRoleRequestValidator : AbstractValidator<AssignServiceRoleRequest>
{
    private static readonly string[] ValidRoles = Enum.GetNames<GroupServiceRoleType>();

    /// <summary>Initializes validation rules for <see cref="AssignServiceRoleRequest"/>.</summary>
    public AssignServiceRoleRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.RoleType)
            .NotEmpty()
            .Must(r => ValidRoles.Contains(r, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"RoleType must be one of: {string.Join(", ", ValidRoles)}");
        RuleFor(x => x.CustomTitle)
            .MaximumLength(100)
            .When(x => x.CustomTitle != null);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

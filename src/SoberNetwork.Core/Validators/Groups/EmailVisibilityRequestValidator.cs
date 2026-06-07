using FluentValidation;
using SoberNetwork.Core.DTOs.Groups;

namespace SoberNetwork.Core.Validators.Groups;

/// <summary>Validates requests that toggle per-group email sharing.</summary>
public class EmailVisibilityRequestValidator : AbstractValidator<EmailVisibilityRequest>
{
    /// <summary>Initializes validation rules for <see cref="EmailVisibilityRequest"/>.</summary>
    public EmailVisibilityRequestValidator()
    {
        RuleFor(x => x.IsShared).NotNull();
    }
}

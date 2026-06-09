using FluentValidation;
using SoberNetwork.Core.DTOs.Groups;

namespace SoberNetwork.Core.Validators.Groups;

public class CreateGroupRequestValidator : AbstractValidator<CreateGroupRequest>
{
    public CreateGroupRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(50)
            .Matches("^[a-z0-9-]+$")
            .WithMessage("Slug may only contain lowercase letters, numbers, and hyphens.");
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.TimeZone).MaximumLength(100);
        RuleFor(x => x.DistrictName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AreaName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.State).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DistrictWebsiteUrl)
            .MaximumLength(500)
            .Must(url => url == null || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("District website must be a valid URL.");
        RuleFor(x => x.AreaWebsiteUrl)
            .MaximumLength(500)
            .Must(url => url == null || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Area website must be a valid URL.");
        RuleFor(x => x.DistrictLatitude)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("District latitude is required.")
            .InclusiveBetween(-90, 90)
            .WithMessage("Latitude must be between -90 and 90.");
        RuleFor(x => x.DistrictLongitude)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("District longitude is required.")
            .InclusiveBetween(-180, 180)
            .WithMessage("Longitude must be between -180 and 180.");
    }
}

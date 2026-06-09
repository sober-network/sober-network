using FluentValidation;
using SoberNetwork.Core.DTOs.Groups;

namespace SoberNetwork.Core.Validators.Groups;

public class UpdateGroupRequestValidator : AbstractValidator<UpdateGroupRequest>
{
    public UpdateGroupRequestValidator()
    {
        RuleFor(x => x.Name).MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.TimeZone).MaximumLength(100);
        RuleFor(x => x.DistrictName).MaximumLength(100);
        RuleFor(x => x.AreaName).MaximumLength(100);
        RuleFor(x => x.State).MaximumLength(50);
        RuleFor(x => x.DistrictWebsiteUrl)
            .MaximumLength(500)
            .Must(url => url == null || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("District website must be a valid URL.");
        RuleFor(x => x.AreaWebsiteUrl)
            .MaximumLength(500)
            .Must(url => url == null || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Area website must be a valid URL.");
        RuleFor(x => x.DistrictLatitude)
            .InclusiveBetween(-90, 90)
            .When(x => x.DistrictLatitude.HasValue)
            .WithMessage("Latitude must be between -90 and 90.");
        RuleFor(x => x.DistrictLongitude)
            .InclusiveBetween(-180, 180)
            .When(x => x.DistrictLongitude.HasValue)
            .WithMessage("Longitude must be between -180 and 180.");
    }
}

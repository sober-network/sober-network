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
        RuleFor(x => x.MeetingSchedule).MaximumLength(1000);
        RuleFor(x => x.MeetingTime).MaximumLength(5);
        RuleFor(x => x.Language).MaximumLength(100);
        RuleFor(x => x.MeetingFormats).MaximumLength(500);
        RuleFor(x => x.ZoomLink).MaximumLength(500);
        RuleFor(x => x.ZoomMeetingId).MaximumLength(100);
        RuleFor(x => x.ZoomPasscode).MaximumLength(100);
        RuleFor(x => x.TimeZone).MaximumLength(100);
    }
}

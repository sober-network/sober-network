using FluentValidation;
using SoberNetwork.Core.DTOs.Groups;

namespace SoberNetwork.Core.Validators.Groups;

public class UpdateGroupRequestValidator : AbstractValidator<UpdateGroupRequest>
{
    public UpdateGroupRequestValidator()
    {
        RuleFor(x => x.Name).MaximumLength(100);
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

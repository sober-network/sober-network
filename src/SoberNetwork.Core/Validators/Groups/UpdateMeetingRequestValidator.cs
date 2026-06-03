using FluentValidation;
using SoberNetwork.Core.DTOs.Groups;

namespace SoberNetwork.Core.Validators.Groups;

public class UpdateMeetingRequestValidator : AbstractValidator<UpdateMeetingRequest>
{
    private static readonly HashSet<string> ValidFormats = new(StringComparer.OrdinalIgnoreCase)
        { "Discussion", "Speaker", "StepStudy", "BigBook", "Beginners" };

    public UpdateMeetingRequestValidator()
    {
        RuleFor(x => x.Name).MaximumLength(100).When(x => x.Name is not null);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.Time)
            .Matches(@"^([01]\d|2[0-3]):[0-5]\d$")
            .WithMessage("Time must be in HH:mm 24-hour format (e.g. 07:00).")
            .When(x => x.Time is not null);
        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween(1, 480)
            .When(x => x.DurationMinutes.HasValue);
        RuleFor(x => x.DayOfWeek)
            .InclusiveBetween(0, 6)
            .WithMessage("DayOfWeek must be between 0 (Sunday) and 6 (Saturday).")
            .When(x => x.DayOfWeek.HasValue);
        RuleForEach(x => x.Formats)
            .Must(f => ValidFormats.Contains(f))
            .WithMessage("'{PropertyValue}' is not a valid meeting format. Valid values: Discussion, Speaker, StepStudy, BigBook, Beginners.")
            .When(x => x.Formats is not null);
        RuleFor(x => x.Language).MaximumLength(100);
        RuleFor(x => x.Location).MaximumLength(500);
        RuleFor(x => x.ZoomLink).MaximumLength(500);
        RuleFor(x => x.ZoomMeetingId).MaximumLength(100);
        RuleFor(x => x.ZoomPasscode).MaximumLength(100);

        // When IsRecurring is being changed, enforce co-field requirements.
        When(x => x.IsRecurring == true, () =>
        {
            RuleFor(x => x.OccursOn)
                .Null().WithMessage("OccursOn must be null when setting a meeting to recurring.");
        });

        When(x => x.IsRecurring == false, () =>
        {
            RuleFor(x => x.DayOfWeek)
                .Null().WithMessage("DayOfWeek must be null when setting a meeting to one-off.");
        });
    }
}

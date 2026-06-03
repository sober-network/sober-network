using FluentValidation;
using SoberNetwork.Core.DTOs.Groups;

namespace SoberNetwork.Core.Validators.Groups;

public class CreateMeetingRequestValidator : AbstractValidator<CreateMeetingRequest>
{
    private static readonly HashSet<string> ValidFormats = new(StringComparer.OrdinalIgnoreCase)
        { "Discussion", "Speaker", "StepStudy", "BigBook", "Beginners" };

    public CreateMeetingRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.Time)
            .NotEmpty()
            .Matches(@"^([01]\d|2[0-3]):[0-5]\d$")
            .WithMessage("Time must be in HH:mm 24-hour format (e.g. 07:00).");
        RuleFor(x => x.DurationMinutes).InclusiveBetween(1, 480);
        RuleForEach(x => x.Formats)
            .Must(f => ValidFormats.Contains(f))
            .WithMessage("'{PropertyValue}' is not a valid meeting format. Valid values: Discussion, Speaker, StepStudy, BigBook, Beginners.");
        RuleFor(x => x.Language).MaximumLength(100);
        RuleFor(x => x.Location).MaximumLength(500);
        RuleFor(x => x.ZoomLink).MaximumLength(500);
        RuleFor(x => x.ZoomMeetingId).MaximumLength(100);
        RuleFor(x => x.ZoomPasscode).MaximumLength(100);

        // Recurring meetings require DayOfWeek; one-off meetings require OccursOn.
        When(x => x.IsRecurring, () =>
        {
            RuleFor(x => x.DayOfWeek)
                .NotNull().WithMessage("DayOfWeek is required for recurring meetings.")
                .InclusiveBetween(0, 6).WithMessage("DayOfWeek must be between 0 (Sunday) and 6 (Saturday).");
            RuleFor(x => x.OccursOn)
                .Null().WithMessage("OccursOn must be null for recurring meetings.");
        });

        When(x => !x.IsRecurring, () =>
        {
            RuleFor(x => x.OccursOn)
                .NotNull().WithMessage("OccursOn is required for one-off meetings.");
            RuleFor(x => x.DayOfWeek)
                .Null().WithMessage("DayOfWeek must be null for one-off meetings.");
        });
    }
}

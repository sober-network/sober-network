using FluentValidation;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Domain.Enums;

namespace SoberNetwork.Core.Validators.Groups;

public class UpdateMeetingRequestValidator : AbstractValidator<UpdateMeetingRequest>
{
    private static readonly HashSet<string> ValidFormats = new(StringComparer.OrdinalIgnoreCase)
        {
            "Discussion",
            "Speaker",
            "StepStudy",
            "TraditionStudy",
            "BigBook",
            "Literature",
            "Topic",
            "Beginners",
            "Candlelight",
            "Meditation",
            "BirthdayChip",
            "Men",
            "Women",
            "YoungPeople",
            "LGBTQPlus"
        };

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
        RuleForEach(x => x.DaysOfWeek)
            .InclusiveBetween(0, 6)
            .WithMessage("Each day of week must be between 0 (Sunday) and 6 (Saturday).")
            .When(x => x.DaysOfWeek is not null);
        RuleForEach(x => x.Formats)
            .Must(f => ValidFormats.Contains(f))
            .WithMessage("'{PropertyValue}' is not a valid meeting format. Valid values: Discussion, Speaker, Step Study, Tradition Study, Big Book Study, Literature, Topic, Beginners, Candlelight, Meditation, Birthday / Chip, Men's Meeting, Women's Meeting, Young People's Meeting, LGBTQ+.")
            .When(x => x.Formats is not null);
        RuleFor(x => x.Language).MaximumLength(100);
        RuleFor(x => x.VenueName).MaximumLength(200);
        RuleFor(x => x.Location).MaximumLength(500);
        RuleFor(x => x.Street).MaximumLength(300);
        RuleFor(x => x.City).MaximumLength(100);
        RuleFor(x => x.State).MaximumLength(100);
        RuleFor(x => x.PostalCode).MaximumLength(20);
        RuleFor(x => x.Country).MaximumLength(100);
        RuleFor(x => x.Latitude).InclusiveBetween(-90.0, 90.0).When(x => x.Latitude.HasValue);
        RuleFor(x => x.Longitude).InclusiveBetween(-180.0, 180.0).When(x => x.Longitude.HasValue);
        RuleFor(x => x.ZoomLink).MaximumLength(500);
        RuleFor(x => x.ZoomMeetingId).MaximumLength(100);
        RuleFor(x => x.ZoomPasscode).MaximumLength(100);
        RuleFor(x => x.PublicJoinUrl)
            .MaximumLength(500)
            .Must(url => url is null || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("PublicJoinUrl must be a valid absolute URL.")
            .When(x => x.PublicJoinUrl is not null);

        // MeetingType invariants when type is being set (only validate when MeetingType is provided).
        When(x => x.MeetingType == MeetingType.Online || x.MeetingType == MeetingType.Hybrid, () =>
        {
            RuleFor(x => x.PublicJoinUrl)
                .NotEmpty().WithMessage("PublicJoinUrl is required when setting meeting type to Online or Hybrid.");
        });

        // When IsRecurring is being changed, enforce co-field requirements.
        When(x => x.IsRecurring == true, () =>
        {
            RuleFor(x => x.OccursOn)
                .Null().WithMessage("OccursOn must be null when setting a meeting to recurring.");
        });

        When(x => x.IsRecurring == false, () =>
        {
            RuleFor(x => x.DaysOfWeek)
                .Null().WithMessage("Days of week must be null when setting a meeting to one-off.");
        });
    }
}

using FluentValidation;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Domain.Enums;

namespace SoberNetwork.Core.Validators.Groups;

public class CreateMeetingRequestValidator : AbstractValidator<CreateMeetingRequest>
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

    public CreateMeetingRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Meeting name is required.")
            .MaximumLength(100).WithMessage("Meeting name must not exceed 100 characters.");
        
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");
        
        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Admin notes must not exceed 2000 characters.");
        
        RuleFor(x => x.Time)
            .NotEmpty().WithMessage("Start time is required.")
            .Matches(@"^([01]\d|2[0-3]):[0-5]\d$")
            .WithMessage("Start time must be in HH:mm format (e.g., 07:00 or 14:30).");
        
        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween(1, 480)
            .WithMessage("Meeting duration must be between 1 and 480 minutes.");
        
        RuleForEach(x => x.Formats)
            .Must(f => ValidFormats.Contains(f))
            .WithMessage("Invalid meeting format '{PropertyValue}'. Valid formats: Discussion, Speaker, Step Study, Tradition Study, Big Book Study, Literature, Topic, Beginners, Candlelight, Meditation, Birthday / Chip, Men's Meeting, Women's Meeting, Young People's Meeting, LGBTQ+.");
        
        RuleFor(x => x.Language)
            .MaximumLength(100).WithMessage("Language must not exceed 100 characters.");
        
        RuleFor(x => x.VenueName)
            .MaximumLength(200).WithMessage("Venue name must not exceed 200 characters.");
        
        RuleFor(x => x.Location)
            .MaximumLength(500).WithMessage("Location description must not exceed 500 characters.");
        
        RuleFor(x => x.Street)
            .MaximumLength(300).WithMessage("Street address must not exceed 300 characters.");
        
        RuleFor(x => x.Street2)
            .MaximumLength(300).WithMessage("Address line 2 must not exceed 300 characters.");
        
        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("City must not exceed 100 characters.");
        
        RuleFor(x => x.State)
            .MaximumLength(100).WithMessage("State/Province must not exceed 100 characters.");
        
        RuleFor(x => x.PostalCode)
            .MaximumLength(20).WithMessage("Postal code must not exceed 20 characters.");
        
        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters.");
        
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90.0, 90.0)
            .When(x => x.Latitude.HasValue)
            .WithMessage("Latitude must be between -90 and 90.");
        
        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180.0, 180.0)
            .When(x => x.Longitude.HasValue)
            .WithMessage("Longitude must be between -180 and 180.");
        
        RuleFor(x => x.ZoomLink)
            .MaximumLength(500).WithMessage("Zoom link must not exceed 500 characters.");
        
        RuleFor(x => x.ZoomMeetingId)
            .MaximumLength(100).WithMessage("Zoom meeting ID must not exceed 100 characters.");
        
        RuleFor(x => x.ZoomPasscode)
            .MaximumLength(100).WithMessage("Zoom passcode must not exceed 100 characters.");
        
        RuleFor(x => x.PublicJoinUrl)
            .MaximumLength(500).WithMessage("Public join URL must not exceed 500 characters.");

        // Address requirements based on meeting type
        When(x => x.MeetingType == MeetingType.InPerson || x.MeetingType == MeetingType.Hybrid, () =>
        {
            RuleFor(x => x.Street)
                .NotEmpty().WithMessage("Street address is required for in-person and hybrid meetings.");
            RuleFor(x => x.City)
                .NotEmpty().WithMessage("City is required for in-person and hybrid meetings.");
        });

        When(x => x.MeetingType == MeetingType.Online || x.MeetingType == MeetingType.Hybrid, () =>
        {
            RuleFor(x => x.PublicJoinUrl)
                .NotEmpty().WithMessage("Join URL is required for online and hybrid meetings.")
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("Join URL must be a valid link (e.g., https://zoom.us/...).");
        });

        // Schedule validation: Recurring meetings require days; one-off meetings require date
        When(x => x.IsRecurring, () =>
        {
            RuleFor(x => x.DaysOfWeek)
                .NotEmpty().WithMessage("Select at least one day of the week for recurring meetings.")
                .Must(days => days == null || (days.All(d => d >= 0 && d <= 6)))
                .WithMessage("Invalid day of week (must be 0-6).");
            RuleFor(x => x.OccursOn)
                .Null().WithMessage("One-off date should not be set for recurring meetings.");
        });

        When(x => !x.IsRecurring, () =>
        {
            RuleFor(x => x.OccursOn)
                .NotNull().WithMessage("Meeting date is required for one-time meetings.");
            RuleFor(x => x.DaysOfWeek)
                .Null().WithMessage("Days of week should not be set for one-time meetings.");
        });
    }
}

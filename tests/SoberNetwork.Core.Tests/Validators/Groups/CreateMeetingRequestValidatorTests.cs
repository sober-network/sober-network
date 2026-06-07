using FluentValidation.TestHelper;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Validators.Groups;
using SoberNetwork.Domain.Enums;

namespace SoberNetwork.Core.Tests.Validators.Groups;

public class CreateMeetingRequestValidatorTests
{
    private readonly CreateMeetingRequestValidator _validator = new();

    [Fact]
    public void valid_request_passes()
    {
        // Arrange
        var request = new CreateMeetingRequest(
            Name: "Monday Night Meeting",
            Description: "A great meeting",
            Notes: "Admin notes",
            IsRecurring: true,
            DaysOfWeek: new[] { 1 }, // Monday
            Time: "19:00",
            DurationMinutes: 60,
            OccursOn: null,
            Formats: new[] { "Discussion", "Speaker" },
            MeetingType: MeetingType.InPerson,
            Street: "123 Main St",
            City: "New York",
            State: "NY",
            PostalCode: "10001",
            Country: "USA",
            PublicJoinUrl: "https://example.com/join"
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void empty_name_fails()
    {
        // Arrange
        var request = new CreateMeetingRequest(
            Name: string.Empty,
            Time: "19:00"
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("25:00")]
    [InlineData("24:01")]
    [InlineData("invalid")]
    public void invalid_time_fails(string time)
    {
        // Arrange
        var request = new CreateMeetingRequest(
            Name: "Meeting",
            Time: time
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Time);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(481)]
    public void duration_out_of_range_fails(int duration)
    {
        // Arrange
        var request = new CreateMeetingRequest(
            Name: "Meeting",
            Time: "19:00",
            DurationMinutes: duration
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DurationMinutes);
    }

    [Fact]
    public void invalid_format_fails()
    {
        // Arrange
        var request = new CreateMeetingRequest(
            Name: "Meeting",
            Time: "19:00",
            Formats: new[] { "InvalidFormat" }
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Formats);
    }
}

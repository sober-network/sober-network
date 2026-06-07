using FluentValidation.TestHelper;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Validators.Groups;
using SoberNetwork.Domain.Enums;

namespace SoberNetwork.Core.Tests.Validators.Groups;

public class UpdateMeetingRequestValidatorTests
{
    private readonly UpdateMeetingRequestValidator _validator = new();

    [Fact]
    public void valid_request_passes()
    {
        // Arrange
        var request = new UpdateMeetingRequest
        {
            Name = "Monday Night Meeting",
            Description = "A great meeting",
            Notes = "Admin notes",
            Time = "19:00",
            DurationMinutes = 60,
            Formats = new[] { "Discussion", "Speaker" }
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void name_too_long_fails()
    {
        // Arrange
        var request = new UpdateMeetingRequest { Name = new string('X', 101) };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void description_too_long_fails()
    {
        // Arrange
        var request = new UpdateMeetingRequest { Description = new string('X', 1001) };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Theory]
    [InlineData("25:00")]
    [InlineData("24:01")]
    [InlineData("99:99")]
    [InlineData("invalid")]
    public void invalid_time_fails(string time)
    {
        // Arrange
        var request = new UpdateMeetingRequest { Time = time };

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
        var request = new UpdateMeetingRequest { DurationMinutes = duration };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DurationMinutes);
    }

    [Fact]
    public void invalid_format_fails()
    {
        // Arrange
        var request = new UpdateMeetingRequest { Formats = new[] { "InvalidFormat" } };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Formats);
    }

    [Theory]
    [InlineData(-91.0)]
    [InlineData(91.0)]
    public void latitude_out_of_range_fails(double lat)
    {
        // Arrange
        var request = new UpdateMeetingRequest { Latitude = lat };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Latitude);
    }

    [Theory]
    [InlineData(-181.0)]
    [InlineData(181.0)]
    public void longitude_out_of_range_fails(double lon)
    {
        // Arrange
        var request = new UpdateMeetingRequest { Longitude = lon };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Longitude);
    }

    [Fact]
    public void invalid_url_fails()
    {
        // Arrange
        var request = new UpdateMeetingRequest { PublicJoinUrl = "not-a-url" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PublicJoinUrl);
    }
}

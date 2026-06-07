using FluentValidation.TestHelper;
using SoberNetwork.Core.DTOs.Members;
using SoberNetwork.Core.Validators.Members;

namespace SoberNetwork.Core.Tests.Validators.Members;

public class UpdateProfileRequestValidatorTests
{
    private readonly UpdateProfileRequestValidator _validator = new();

    [Fact]
    public void valid_request_passes()
    {
        // Arrange
        var request = new UpdateProfileRequest("John Doe", "John", "America/New_York");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void null_values_pass()
    {
        // Arrange - all optional fields can be null
        var request = new UpdateProfileRequest(null, null, null);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void empty_values_pass()
    {
        // Arrange - MaximumLength validator doesn't validate empty strings
        var request = new UpdateProfileRequest(string.Empty, string.Empty, string.Empty);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void display_name_at_max_length_passes()
    {
        // Arrange
        var request = new UpdateProfileRequest(new string('X', 100), "John", "America/New_York");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void display_name_too_long_fails()
    {
        // Arrange
        var request = new UpdateProfileRequest(new string('X', 101), "John", "America/New_York");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DisplayName);
    }

    [Fact]
    public void first_name_at_max_length_passes()
    {
        // Arrange
        var request = new UpdateProfileRequest("John Doe", new string('X', 50), "America/New_York");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void first_name_too_long_fails()
    {
        // Arrange
        var request = new UpdateProfileRequest("John Doe", new string('X', 51), "America/New_York");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void timezone_too_long_fails()
    {
        // Arrange
        var request = new UpdateProfileRequest("John Doe", "John", new string('X', 101));

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TimeZone);
    }
}

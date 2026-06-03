using FluentValidation.TestHelper;
using SoberNetwork.Core.DTOs.Auth;
using SoberNetwork.Core.Validators.Auth;

namespace SoberNetwork.Core.Tests.Validators.Auth;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    [Fact]
    public void valid_request_passes()
    {
        // Arrange
        var request = new RegisterRequest("user@example.com", "Password1!@#", "John D.", null);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void password_too_short_fails()
    {
        // Arrange
        var request = new RegisterRequest("user@example.com", "short", "John D.", null);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void missing_display_name_fails()
    {
        // Arrange
        var request = new RegisterRequest("user@example.com", "Password1!@#", string.Empty, null);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DisplayName);
    }

    [Fact]
    public void invalid_email_fails()
    {
        // Arrange
        var request = new RegisterRequest("bad-email", "Password1!@#", "John D.", null);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }
}

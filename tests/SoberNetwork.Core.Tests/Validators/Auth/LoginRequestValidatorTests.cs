using FluentValidation.TestHelper;
using SoberNetwork.Core.DTOs.Auth;
using SoberNetwork.Core.Validators.Auth;

namespace SoberNetwork.Core.Tests.Validators.Auth;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void valid_request_passes()
    {
        // Arrange
        var request = new LoginRequest("user@example.com", "password123");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("notanemail")]
    [InlineData("missing@")] 
    public void invalid_email_fails(string email)
    {
        // Arrange
        var request = new LoginRequest(email, "password123");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void empty_password_fails()
    {
        // Arrange
        var request = new LoginRequest("user@example.com", string.Empty);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}

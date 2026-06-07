using FluentValidation.TestHelper;
using SoberNetwork.Core.DTOs.Auth;
using SoberNetwork.Core.Validators.Auth;

namespace SoberNetwork.Core.Tests.Validators.Auth;

public class ForgotPasswordRequestValidatorTests
{
    private readonly ForgotPasswordRequestValidator _validator = new();

    [Fact]
    public void valid_request_passes()
    {
        // Arrange
        var request = new ForgotPasswordRequest("user@example.com");

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
        var request = new ForgotPasswordRequest(email);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }
}

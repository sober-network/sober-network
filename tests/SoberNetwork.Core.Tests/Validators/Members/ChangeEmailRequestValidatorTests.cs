using FluentValidation.TestHelper;
using SoberNetwork.Core.DTOs.Members;
using SoberNetwork.Core.Validators.Members;

namespace SoberNetwork.Core.Tests.Validators.Members;

public class ChangeEmailRequestValidatorTests
{
    private readonly ChangeEmailRequestValidator _validator = new();

    [Fact]
    public void valid_request_passes()
    {
        // Arrange
        var request = new ChangeEmailRequest("password123", "newemail@example.com");

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
        var request = new ChangeEmailRequest("password123", email);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewEmail);
    }

    [Fact]
    public void empty_password_fails()
    {
        // Arrange
        var request = new ChangeEmailRequest(string.Empty, "newemail@example.com");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CurrentPassword);
    }
}

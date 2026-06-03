using FluentValidation.TestHelper;
using SoberNetwork.Core.DTOs.Auth;
using SoberNetwork.Core.Validators.Auth;

namespace SoberNetwork.Core.Tests.Validators.Auth;

public class ResetPasswordRequestValidatorTests
{
    private readonly ResetPasswordRequestValidator _validator = new();

    [Fact]
    public void valid_request_passes()
    {
        // Arrange
        var request = new ResetPasswordRequest("user-id", "valid-token", "NewPassword1!");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void missing_user_id_fails()
    {
        // Arrange
        var request = new ResetPasswordRequest(string.Empty, "token", "NewPassword1!");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void missing_token_fails()
    {
        // Arrange
        var request = new ResetPasswordRequest("user-id", string.Empty, "NewPassword1!");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Token);
    }

    [Fact]
    public void short_password_fails()
    {
        // Arrange
        var request = new ResetPasswordRequest("user-id", "token", "short");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }
}

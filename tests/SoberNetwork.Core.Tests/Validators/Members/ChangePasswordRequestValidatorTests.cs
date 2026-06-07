using FluentValidation.TestHelper;
using SoberNetwork.Core.DTOs.Members;
using SoberNetwork.Core.Validators.Members;

namespace SoberNetwork.Core.Tests.Validators.Members;

public class ChangePasswordRequestValidatorTests
{
    private readonly ChangePasswordRequestValidator _validator = new();

    [Fact]
    public void valid_request_passes()
    {
        // Arrange
        var request = new ChangePasswordRequest("oldpassword123", "newpassword123", "newpassword123");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void empty_current_password_fails()
    {
        // Arrange
        var request = new ChangePasswordRequest(string.Empty, "newpassword123", "newpassword123");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CurrentPassword);
    }

    [Fact]
    public void empty_new_password_fails()
    {
        // Arrange
        var request = new ChangePasswordRequest("oldpassword123", string.Empty, "newpassword123");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void new_password_too_short_fails()
    {
        // Arrange
        var request = new ChangePasswordRequest("oldpassword123", "short1", "short1");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void empty_confirm_new_password_fails()
    {
        // Arrange
        var request = new ChangePasswordRequest("oldpassword123", "newpassword123", string.Empty);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConfirmNewPassword);
    }
}

using FluentValidation.TestHelper;
using SoberNetwork.Core.DTOs.Members;
using SoberNetwork.Core.Validators.Members;

namespace SoberNetwork.Core.Tests.Validators.Members;

public class DeleteAccountRequestValidatorTests
{
    private readonly DeleteAccountRequestValidator _validator = new();

    [Fact]
    public void valid_request_passes()
    {
        // Arrange
        var request = new DeleteAccountRequest("password123");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void empty_password_fails()
    {
        // Arrange
        var request = new DeleteAccountRequest(string.Empty);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}

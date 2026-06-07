using FluentValidation.TestHelper;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Validators.Groups;

namespace SoberNetwork.Core.Tests.Validators.Groups;

public class PhoneVisibilityRequestValidatorTests
{
    private readonly PhoneVisibilityRequestValidator _validator = new();

    [Fact]
    public void valid_request_shared_passes()
    {
        // Arrange
        var request = new PhoneVisibilityRequest(true);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void valid_request_not_shared_passes()
    {
        // Arrange
        var request = new PhoneVisibilityRequest(false);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}

using FluentValidation.TestHelper;
using SoberNetwork.Core.DTOs.Members;
using SoberNetwork.Core.Validators.Members;

namespace SoberNetwork.Core.Tests.Validators.Members;

public class SobrietyVisibilityRequestValidatorTests
{
    private readonly SobrietyVisibilityRequestValidator _validator = new();

    [Fact]
    public void valid_request_public_passes()
    {
        // Arrange
        var request = new SobrietyVisibilityRequest(true);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void valid_request_private_passes()
    {
        // Arrange
        var request = new SobrietyVisibilityRequest(false);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}

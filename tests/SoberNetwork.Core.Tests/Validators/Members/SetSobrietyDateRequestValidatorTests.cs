using FluentValidation.TestHelper;
using SoberNetwork.Core.DTOs.Members;
using SoberNetwork.Core.Validators.Members;

namespace SoberNetwork.Core.Tests.Validators.Members;

public class SetSobrietyDateRequestValidatorTests
{
    private readonly SetSobrietyDateRequestValidator _validator = new();

    [Fact]
    public void valid_request_passes()
    {
        // Arrange
        var request = new SetSobrietyDateRequest(new DateOnly(2020, 1, 15));

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}

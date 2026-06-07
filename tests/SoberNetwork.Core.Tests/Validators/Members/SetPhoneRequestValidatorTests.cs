using FluentValidation.TestHelper;
using SoberNetwork.Core.DTOs.Members;
using SoberNetwork.Core.Validators.Members;

namespace SoberNetwork.Core.Tests.Validators.Members;

public class SetPhoneRequestValidatorTests
{
    private readonly SetPhoneRequestValidator _validator = new();

    [Fact]
    public void valid_request_passes()
    {
        // Arrange
        var request = new SetPhoneRequest("(555) 123-4567");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void empty_phone_fails()
    {
        // Arrange
        var request = new SetPhoneRequest(string.Empty);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void phone_too_long_fails()
    {
        // Arrange
        var request = new SetPhoneRequest(new string('5', 21));

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }
}

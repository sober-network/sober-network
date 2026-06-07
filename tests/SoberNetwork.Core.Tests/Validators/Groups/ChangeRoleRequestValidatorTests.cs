using FluentValidation.TestHelper;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Validators.Groups;
using SoberNetwork.Domain.Enums;

namespace SoberNetwork.Core.Tests.Validators.Groups;

public class ChangeRoleRequestValidatorTests
{
    private readonly ChangeRoleRequestValidator _validator = new();

    [Fact]
    public void valid_request_passes()
    {
        // Arrange
        var request = new ChangeRoleRequest(GroupRole.Member);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(GroupRole.GroupAdmin)]
    [InlineData(GroupRole.Member)]
    public void all_valid_roles_pass(GroupRole role)
    {
        // Arrange
        var request = new ChangeRoleRequest(role);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}

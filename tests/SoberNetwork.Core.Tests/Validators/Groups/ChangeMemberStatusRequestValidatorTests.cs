using FluentValidation.TestHelper;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Validators.Groups;
using SoberNetwork.Domain.Enums;

namespace SoberNetwork.Core.Tests.Validators.Groups;

public class ChangeMemberStatusRequestValidatorTests
{
    private readonly ChangeMemberStatusRequestValidator _validator = new();

    [Fact]
    public void valid_request_passes()
    {
        // Arrange
        var request = new ChangeMemberStatusRequest(MemberStatus.Active);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(MemberStatus.Active)]
    [InlineData(MemberStatus.Suspended)]
    [InlineData(MemberStatus.Banned)]
    [InlineData(MemberStatus.PendingApproval)]
    public void all_valid_statuses_pass(MemberStatus status)
    {
        // Arrange
        var request = new ChangeMemberStatusRequest(status);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}

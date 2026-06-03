using Moq;
using SoberNetwork.Core.Commands.Groups;
using SoberNetwork.Core.Handlers.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Tests.Handlers.Groups;

public class ClearProbationCommandHandlerTests
{
    private readonly Mock<IGroupService> _groupService = new();
    private readonly ClearProbationCommandHandler _handler;

    public ClearProbationCommandHandlerTests() => _handler = new ClearProbationCommandHandler(_groupService.Object);

    [Fact]
    public async Task success_returns_ok()
    {
        // Arrange
        var command = new ClearProbationCommand("group-slug", "target-user-id", "admin-user-id");
        _groupService
            .Setup(service => service.ClearProbationaryStatusAsync(command.Slug, command.TargetUserId, command.AdminUserId))
            .ReturnsAsync((true, (string?)null));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(ResultCode.Ok, result.Code);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task permission_error_returns_forbidden()
    {
        // Arrange
        var command = new ClearProbationCommand("group-slug", "target-user-id", "user-id");
        const string error = "You do not have permission to clear probationary status.";

        _groupService
            .Setup(service => service.ClearProbationaryStatusAsync(command.Slug, command.TargetUserId, command.AdminUserId))
            .ReturnsAsync((false, error));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ResultCode.Forbidden, result.Code);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public async Task other_error_returns_bad_request()
    {
        // Arrange
        var command = new ClearProbationCommand("group-slug", "target-user-id", "admin-user-id");
        const string error = "Member is not on probation.";

        _groupService
            .Setup(service => service.ClearProbationaryStatusAsync(command.Slug, command.TargetUserId, command.AdminUserId))
            .ReturnsAsync((false, error));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ResultCode.BadRequest, result.Code);
        Assert.Equal(error, result.Error);
    }
}

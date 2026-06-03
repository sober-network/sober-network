using Moq;
using SoberNetwork.Core.Commands.Groups;
using SoberNetwork.Core.Handlers.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Tests.Handlers.Groups;

public class LeaveGroupCommandHandlerTests
{
    private readonly Mock<IGroupService> _groupService = new();
    private readonly LeaveGroupCommandHandler _handler;

    public LeaveGroupCommandHandlerTests() => _handler = new LeaveGroupCommandHandler(_groupService.Object);

    [Fact]
    public async Task success_returns_ok()
    {
        // Arrange
        var command = new LeaveGroupCommand("group-slug", "user-id");
        _groupService
            .Setup(service => service.LeaveGroupAsync(command.Slug, command.UserId))
            .ReturnsAsync((true, (string?)null));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(ResultCode.Ok, result.Code);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task only_admin_error_returns_conflict()
    {
        // Arrange
        var command = new LeaveGroupCommand("group-slug", "user-id");
        const string error = "You are the only admin for this group.";

        _groupService
            .Setup(service => service.LeaveGroupAsync(command.Slug, command.UserId))
            .ReturnsAsync((false, error));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ResultCode.Conflict, result.Code);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public async Task other_error_returns_not_found()
    {
        // Arrange
        var command = new LeaveGroupCommand("missing-group", "user-id");
        const string error = "Group not found.";

        _groupService
            .Setup(service => service.LeaveGroupAsync(command.Slug, command.UserId))
            .ReturnsAsync((false, error));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ResultCode.NotFound, result.Code);
        Assert.Equal(error, result.Error);
    }
}

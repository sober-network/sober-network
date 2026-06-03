using Moq;
using SoberNetwork.Core.Commands.Groups;
using SoberNetwork.Core.Handlers.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Tests.Handlers.Groups;

public class DeleteGroupCommandHandlerTests
{
    private readonly Mock<IGroupService> _groupService = new();
    private readonly DeleteGroupCommandHandler _handler;

    public DeleteGroupCommandHandlerTests() => _handler = new DeleteGroupCommandHandler(_groupService.Object);

    [Fact]
    public async Task success_returns_ok()
    {
        // Arrange
        var command = new DeleteGroupCommand("group-slug", "admin-user-id");
        _groupService
            .Setup(service => service.SoftDeleteGroupAsync(command.Slug, command.UserId))
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
        var command = new DeleteGroupCommand("group-slug", "user-id");
        const string error = "You do not have permission to delete this group.";

        _groupService
            .Setup(service => service.SoftDeleteGroupAsync(command.Slug, command.UserId))
            .ReturnsAsync((false, error));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ResultCode.Forbidden, result.Code);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public async Task other_error_returns_not_found()
    {
        // Arrange
        var command = new DeleteGroupCommand("missing-group", "admin-user-id");
        const string error = "Group not found.";

        _groupService
            .Setup(service => service.SoftDeleteGroupAsync(command.Slug, command.UserId))
            .ReturnsAsync((false, error));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ResultCode.NotFound, result.Code);
        Assert.Equal(error, result.Error);
    }
}

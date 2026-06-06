using Moq;
using SoberNetwork.Core.Commands.Groups;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Handlers.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Tests.Handlers.Groups;

public class UpdateGroupCommandHandlerTests
{
    private readonly Mock<IGroupService> _groupService = new();
    private readonly UpdateGroupCommandHandler _handler;

    public UpdateGroupCommandHandlerTests() => _handler = new UpdateGroupCommandHandler(_groupService.Object);

    [Fact]
    public async Task success_returns_ok_with_data()
    {
        // Arrange
        var request = new UpdateGroupRequest("Updated Group", null, null, null, null);
        var command = new UpdateGroupCommand("group-slug", request, Guid.Parse("00000000-0000-0000-0000-000000000001"));
        var response = new GroupResponse(Guid.NewGuid(), "Updated Group", "group-slug", null, null, true, true, true, 5, "GroupAdmin", "Active", DateTime.UtcNow, []);

        _groupService
            .Setup(service => service.UpdateGroupAsync(command.Slug, command.Request, command.UserId))
            .ReturnsAsync((response, (string?)null));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(ResultCode.Ok, result.Code);
        Assert.Equal(response, result.Data);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task permission_error_returns_forbidden()
    {
        // Arrange
        var request = new UpdateGroupRequest("Updated Group", null, null, null, null);
        var command = new UpdateGroupCommand("group-slug", request, Guid.Parse("00000000-0000-0000-0000-000000000001"));
        const string error = "You do not have permission to update this group.";

        _groupService
            .Setup(service => service.UpdateGroupAsync(command.Slug, command.Request, command.UserId))
            .ReturnsAsync(((GroupResponse?)null, error));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ResultCode.Forbidden, result.Code);
        Assert.Equal(error, result.Error);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task other_error_returns_not_found()
    {
        // Arrange
        var request = new UpdateGroupRequest("Updated Group", null, null, null, null);
        var command = new UpdateGroupCommand("missing-group", request, Guid.Parse("00000000-0000-0000-0000-000000000001"));
        const string error = "Group not found.";

        _groupService
            .Setup(service => service.UpdateGroupAsync(command.Slug, command.Request, command.UserId))
            .ReturnsAsync(((GroupResponse?)null, error));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ResultCode.NotFound, result.Code);
        Assert.Equal(error, result.Error);
        Assert.Null(result.Data);
    }
}

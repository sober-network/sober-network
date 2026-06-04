using Moq;
using SoberNetwork.Core.Commands.Groups;
using SoberNetwork.Core.Handlers.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Tests.Handlers.Groups;

public class RequestToJoinCommandHandlerTests
{
    private readonly Mock<IGroupService> _groupService = new();
    private readonly RequestToJoinCommandHandler _handler;

    public RequestToJoinCommandHandlerTests() => _handler = new RequestToJoinCommandHandler(_groupService.Object);

    [Fact]
    public async Task success_with_auto_approved_returns_ok_true()
    {
        // Arrange
        var command = new RequestToJoinCommand("group-slug", Guid.Parse("00000000-0000-0000-0000-000000000001"));
        _groupService
            .Setup(service => service.RequestToJoinAsync(command.Slug, command.UserId))
            .ReturnsAsync((true, true, (string?)null));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(ResultCode.Ok, result.Code);
        Assert.True(result.Data);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task success_without_auto_approved_returns_ok_false()
    {
        // Arrange
        var command = new RequestToJoinCommand("group-slug", Guid.Parse("00000000-0000-0000-0000-000000000001"));
        _groupService
            .Setup(service => service.RequestToJoinAsync(command.Slug, command.UserId))
            .ReturnsAsync((true, false, (string?)null));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(ResultCode.Ok, result.Code);
        Assert.False(result.Data);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task already_a_member_error_returns_conflict()
    {
        // Arrange
        var command = new RequestToJoinCommand("group-slug", Guid.Parse("00000000-0000-0000-0000-000000000001"));
        const string error = "User is already a member.";
        _groupService
            .Setup(service => service.RequestToJoinAsync(command.Slug, command.UserId))
            .ReturnsAsync((false, false, error));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ResultCode.Conflict, result.Code);
        Assert.Equal(error, result.Error);
        Assert.False(result.Data);
    }

    [Fact]
    public async Task other_error_returns_not_found()
    {
        // Arrange
        var command = new RequestToJoinCommand("missing-group", Guid.Parse("00000000-0000-0000-0000-000000000001"));
        const string error = "Group not found.";
        _groupService
            .Setup(service => service.RequestToJoinAsync(command.Slug, command.UserId))
            .ReturnsAsync((false, false, error));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ResultCode.NotFound, result.Code);
        Assert.Equal(error, result.Error);
        Assert.False(result.Data);
    }
}

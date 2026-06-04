using Moq;
using SoberNetwork.Core.Commands.Groups;
using SoberNetwork.Core.Handlers.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;
using SoberNetwork.Domain.Enums;

namespace SoberNetwork.Core.Tests.Handlers.Groups;

public class ChangeMemberRoleCommandHandlerTests
{
    private readonly Mock<IGroupService> _groupService = new();
    private readonly ChangeMemberRoleCommandHandler _handler;

    public ChangeMemberRoleCommandHandlerTests() => _handler = new ChangeMemberRoleCommandHandler(_groupService.Object);

    [Fact]
    public async Task success_returns_ok()
    {
        // Arrange
        var command = new ChangeMemberRoleCommand("group-slug", Guid.Parse("00000000-0000-0000-0000-000000000002"), Guid.Parse("00000000-0000-0000-0000-000000000001"), GroupRole.GroupAdmin);
        _groupService
            .Setup(service => service.ChangeRoleAsync(command.Slug, command.TargetUserId, command.AdminUserId, command.NewRole))
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
        var command = new ChangeMemberRoleCommand("group-slug", Guid.Parse("00000000-0000-0000-0000-000000000002"), Guid.Parse("00000000-0000-0000-0000-000000000001"), GroupRole.GroupAdmin);
        const string error = "You do not have permission to change roles.";

        _groupService
            .Setup(service => service.ChangeRoleAsync(command.Slug, command.TargetUserId, command.AdminUserId, command.NewRole))
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
        var command = new ChangeMemberRoleCommand("group-slug", Guid.Parse("00000000-0000-0000-0000-000000000002"), Guid.Parse("00000000-0000-0000-0000-000000000001"), GroupRole.Member);
        const string error = "Cannot demote the last group admin.";

        _groupService
            .Setup(service => service.ChangeRoleAsync(command.Slug, command.TargetUserId, command.AdminUserId, command.NewRole))
            .ReturnsAsync((false, error));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ResultCode.BadRequest, result.Code);
        Assert.Equal(error, result.Error);
    }
}

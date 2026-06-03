using Moq;
using SoberNetwork.Core.Commands.Members;
using SoberNetwork.Core.DTOs.Members;
using SoberNetwork.Core.Handlers.Members;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Tests.Handlers.Members;

public class ChangePasswordCommandHandlerTests
{
    private readonly Mock<IMemberService> _memberService = new();
    private readonly ChangePasswordCommandHandler _handler;

    public ChangePasswordCommandHandlerTests() => _handler = new ChangePasswordCommandHandler(_memberService.Object);

    [Fact]
    public async Task success_returns_ok()
    {
        // Arrange
        var command = new ChangePasswordCommand("user-id", new ChangePasswordRequest("current-password", "NewPassword1!", "NewPassword1!"));
        _memberService
            .Setup(service => service.ChangePasswordAsync(command.UserId, command.Request))
            .ReturnsAsync((true, (string?)null));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(ResultCode.Ok, result.Code);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task match_error_returns_bad_request()
    {
        // Arrange
        var command = new ChangePasswordCommand("user-id", new ChangePasswordRequest("current-password", "NewPassword1!", "DifferentPassword1!"));
        const string error = "New password and confirmation do not match.";

        _memberService
            .Setup(service => service.ChangePasswordAsync(command.UserId, command.Request))
            .ReturnsAsync((false, error));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ResultCode.BadRequest, result.Code);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public async Task non_match_error_returns_unauthorized()
    {
        // Arrange
        var command = new ChangePasswordCommand("user-id", new ChangePasswordRequest("bad-password", "NewPassword1!", "NewPassword1!"));
        const string error = "Incorrect current password.";

        _memberService
            .Setup(service => service.ChangePasswordAsync(command.UserId, command.Request))
            .ReturnsAsync((false, error));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ResultCode.Unauthorized, result.Code);
        Assert.Equal(error, result.Error);
    }
}

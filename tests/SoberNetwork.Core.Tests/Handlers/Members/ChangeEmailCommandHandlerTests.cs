using Moq;
using SoberNetwork.Core.Commands.Members;
using SoberNetwork.Core.DTOs.Members;
using SoberNetwork.Core.Handlers.Members;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Tests.Handlers.Members;

public class ChangeEmailCommandHandlerTests
{
    private readonly Mock<IMemberService> _memberService = new();
    private readonly ChangeEmailCommandHandler _handler;

    public ChangeEmailCommandHandlerTests() => _handler = new ChangeEmailCommandHandler(_memberService.Object);

    [Fact]
    public async Task success_returns_ok()
    {
        // Arrange
        var command = new ChangeEmailCommand("user-id", new ChangeEmailRequest("current-password", "new@example.com"));
        _memberService
            .Setup(service => service.ChangeEmailAsync(command.UserId, command.Request))
            .ReturnsAsync((true, (string?)null));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(ResultCode.Ok, result.Code);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task incorrect_password_error_returns_unauthorized()
    {
        // Arrange
        var command = new ChangeEmailCommand("user-id", new ChangeEmailRequest("bad-password", "new@example.com"));
        const string error = "Incorrect password.";

        _memberService
            .Setup(service => service.ChangeEmailAsync(command.UserId, command.Request))
            .ReturnsAsync((false, error));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ResultCode.Unauthorized, result.Code);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public async Task other_error_returns_bad_request()
    {
        // Arrange
        var command = new ChangeEmailCommand("user-id", new ChangeEmailRequest("current-password", "existing@example.com"));
        const string error = "Email is already in use.";

        _memberService
            .Setup(service => service.ChangeEmailAsync(command.UserId, command.Request))
            .ReturnsAsync((false, error));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ResultCode.BadRequest, result.Code);
        Assert.Equal(error, result.Error);
    }
}

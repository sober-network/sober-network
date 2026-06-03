using Moq;
using SoberNetwork.Core.Commands.Members;
using SoberNetwork.Core.Handlers.Members;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Tests.Handlers.Members;

public class DeleteAccountCommandHandlerTests
{
    private readonly Mock<IMemberService> _memberService = new();
    private readonly DeleteAccountCommandHandler _handler;

    public DeleteAccountCommandHandlerTests() => _handler = new DeleteAccountCommandHandler(_memberService.Object);

    [Fact]
    public async Task success_returns_ok()
    {
        // Arrange
        var command = new DeleteAccountCommand("user-id", "current-password");
        _memberService
            .Setup(service => service.DeleteAccountAsync(command.UserId, command.Password))
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
        var command = new DeleteAccountCommand("user-id", "bad-password");
        const string error = "Incorrect password.";

        _memberService
            .Setup(service => service.DeleteAccountAsync(command.UserId, command.Password))
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
        var command = new DeleteAccountCommand("user-id", "current-password");
        const string error = "Account could not be deleted.";

        _memberService
            .Setup(service => service.DeleteAccountAsync(command.UserId, command.Password))
            .ReturnsAsync((false, error));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ResultCode.BadRequest, result.Code);
        Assert.Equal(error, result.Error);
    }
}

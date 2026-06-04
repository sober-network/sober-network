using Moq;
using SoberNetwork.Core.DTOs.Members;
using SoberNetwork.Core.Handlers.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Queries.Groups;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Tests.Handlers.Groups;

public class GetPhoneListQueryHandlerTests
{
    private readonly Mock<IMemberService> _memberService = new();
    private readonly GetPhoneListQueryHandler _handler;

    public GetPhoneListQueryHandlerTests() => _handler = new GetPhoneListQueryHandler(_memberService.Object);

    [Fact]
    public async Task success_returns_ok_with_data()
    {
        // Arrange
        var query = new GetPhoneListQuery(Guid.Parse("00000000-0000-0000-0000-000000000001"), "group-slug");
        IReadOnlyList<PhoneListEntryResponse> response = [new PhoneListEntryResponse(Guid.Parse("00000000-0000-0000-0000-000000000003"), "John D.", "555-0100")];

        _memberService
            .Setup(service => service.GetGroupPhoneListAsync(query.UserId, query.Slug))
            .ReturnsAsync((response, (string?)null));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(ResultCode.Ok, result.Code);
        Assert.Equal(response, result.Data);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task not_a_member_error_returns_forbidden()
    {
        // Arrange
        var query = new GetPhoneListQuery(Guid.Parse("00000000-0000-0000-0000-000000000001"), "group-slug");
        const string error = "You are not a member of this group.";

        _memberService
            .Setup(service => service.GetGroupPhoneListAsync(query.UserId, query.Slug))
            .ReturnsAsync(((IReadOnlyList<PhoneListEntryResponse>?)null, error));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

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
        var query = new GetPhoneListQuery(Guid.Parse("00000000-0000-0000-0000-000000000001"), "missing-group");
        const string error = "Group not found.";

        _memberService
            .Setup(service => service.GetGroupPhoneListAsync(query.UserId, query.Slug))
            .ReturnsAsync(((IReadOnlyList<PhoneListEntryResponse>?)null, error));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ResultCode.NotFound, result.Code);
        Assert.Equal(error, result.Error);
        Assert.Null(result.Data);
    }
}

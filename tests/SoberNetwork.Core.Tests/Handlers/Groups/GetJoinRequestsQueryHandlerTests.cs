using Moq;
using SoberNetwork.Core.DTOs;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Handlers.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Queries.Groups;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Tests.Handlers.Groups;

public class GetJoinRequestsQueryHandlerTests
{
    private readonly Mock<IGroupService> _groupService = new();
    private readonly GetJoinRequestsQueryHandler _handler;

    public GetJoinRequestsQueryHandlerTests() => _handler = new GetJoinRequestsQueryHandler(_groupService.Object);

    [Fact]
    public async Task success_returns_ok_with_data()
    {
        // Arrange
        var query = new GetJoinRequestsQuery("group-slug", Guid.Parse("00000000-0000-0000-0000-000000000001"), 1, 25);
        var response = new PagedResponse<JoinRequestResponse>(
            [new JoinRequestResponse(Guid.Parse("00000000-0000-0000-0000-000000000003"), "Jane D.", DateTime.UtcNow)],
            1,
            25,
            1);

        _groupService
            .Setup(service => service.GetJoinRequestsAsync(query.Slug, query.UserId, query.Page, query.PageSize))
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
    public async Task permission_error_returns_forbidden()
    {
        // Arrange
        var query = new GetJoinRequestsQuery("group-slug", Guid.Parse("00000000-0000-0000-0000-000000000001"), 1, 25);
        const string error = "You do not have permission to view join requests.";

        _groupService
            .Setup(service => service.GetJoinRequestsAsync(query.Slug, query.UserId, query.Page, query.PageSize))
            .ReturnsAsync(((PagedResponse<JoinRequestResponse>?)null, error));

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
        var query = new GetJoinRequestsQuery("missing-group", Guid.Parse("00000000-0000-0000-0000-000000000001"), 1, 25);
        const string error = "Group not found.";

        _groupService
            .Setup(service => service.GetJoinRequestsAsync(query.Slug, query.UserId, query.Page, query.PageSize))
            .ReturnsAsync(((PagedResponse<JoinRequestResponse>?)null, error));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ResultCode.NotFound, result.Code);
        Assert.Equal(error, result.Error);
        Assert.Null(result.Data);
    }
}

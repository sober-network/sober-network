using System.Net;
using SoberNetwork.Api.Tests.Infrastructure;

namespace SoberNetwork.Api.Tests.Security;

/// <summary>
/// Confirms pagination parameters are validated at the boundary via FluentValidation
/// (PaginationQueryValidator) rather than ad-hoc controller guards.
/// </summary>
public class PaginationValidationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PaginationValidationTests(TestWebApplicationFactory factory)
        => _client = factory.CreateAuthenticatedClient(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    [Theory]
    [InlineData("/api/groups/some-group/members?page=0")]
    [InlineData("/api/groups/some-group/members?pageSize=0")]
    [InlineData("/api/groups/some-group/members?pageSize=101")]
    [InlineData("/api/groups/some-group/join-requests?page=0")]
    [InlineData("/api/groups/some-group/join-requests?pageSize=101")]
    public async Task PaginatedEndpoint_WithInvalidPaging_ReturnsBadRequest(string url)
    {
        var response = await _client.GetAsync(url);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

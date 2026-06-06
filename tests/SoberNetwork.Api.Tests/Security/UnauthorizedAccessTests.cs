using System.Net;
using System.Net.Http.Json;
using Moq;
using SoberNetwork.Api.Tests.Infrastructure;
using SoberNetwork.Core.DTOs.Groups;

namespace SoberNetwork.Api.Tests.Security;

public class UnauthorizedAccessTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UnauthorizedAccessTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateAnonymousClient();
        factory.GroupService
            .Setup(service => service.GetGroupInfoAsync(It.IsAny<string>()))
            .ReturnsAsync((GroupSummaryResponse?)null);
    }

    [Theory]
    [InlineData("GET", "/api/groups")]
    [InlineData("POST", "/api/groups")]
    [InlineData("GET", "/api/groups/all")]
    [InlineData("GET", "/api/groups/some-group")]
    [InlineData("PUT", "/api/groups/some-group")]
    [InlineData("DELETE", "/api/groups/some-group")]
    [InlineData("GET", "/api/groups/some-group/members")]
    [InlineData("DELETE", "/api/groups/some-group/members/me")]
    [InlineData("POST", "/api/groups/some-group/join")]
    [InlineData("GET", "/api/groups/some-group/join-requests")]
    [InlineData("POST", "/api/groups/some-group/members/some-user/approve")]
    [InlineData("POST", "/api/groups/some-group/members/some-user/reject")]
    [InlineData("DELETE", "/api/groups/some-group/members/some-user")]
    [InlineData("PATCH", "/api/groups/some-group/members/some-user/role")]
    [InlineData("PATCH", "/api/groups/some-group/members/some-user/status")]
    [InlineData("PATCH", "/api/groups/some-group/members/some-user/probation")]
    [InlineData("PATCH", "/api/groups/some-group/members/me/phone-visibility")]
    [InlineData("GET", "/api/groups/some-group/phone-list")]
    [InlineData("GET", "/api/groups/some-group/members/some-user")]
    [InlineData("GET", "/api/members/me")]
    [InlineData("GET", "/api/members/me/sobriety")]
    [InlineData("PUT", "/api/members/me")]
    [InlineData("DELETE", "/api/members/me")]
    [InlineData("PATCH", "/api/members/me/password")]
    [InlineData("PATCH", "/api/members/me/email")]
    [InlineData("PUT", "/api/members/me/sobriety-date")]
    [InlineData("DELETE", "/api/members/me/sobriety-date")]
    [InlineData("PATCH", "/api/members/me/sobriety-date/visibility")]
    [InlineData("PUT", "/api/members/me/phone")]
    [InlineData("DELETE", "/api/members/me/phone")]
    [InlineData("GET", "/api/members")]
    [InlineData("GET", "/api/members/some-user")]
    [InlineData("PATCH", "/api/members/some-user/deactivate")]
    public async Task ProtectedEndpoint_WithoutJwt_Returns401(string method, string url)
    {
        // Arrange
        using var request = new HttpRequestMessage(new HttpMethod(method), url);
        if (method is "POST" or "PUT" or "PATCH" || url == "/api/members/me")
        {
            request.Content = JsonContent.Create(new { });
        }

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GroupInfoEndpoint_AsAnonymous_DoesNotReturn401()
    {
        // Act
        var response = await _client.GetAsync("/api/groups/any-slug/info");

        // Assert
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

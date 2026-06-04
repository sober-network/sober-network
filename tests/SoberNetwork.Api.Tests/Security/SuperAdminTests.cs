using System.Net;
using Moq;
using SoberNetwork.Api.Tests.Infrastructure;
using SoberNetwork.Core.DTOs.Groups;

namespace SoberNetwork.Api.Tests.Security;

public class SuperAdminForbiddenTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _regularClient;

    public SuperAdminForbiddenTests(TestWebApplicationFactory factory) => _regularClient = factory.CreateAuthenticatedClient();

    [Fact]
    public async Task get_all_groups_returns_403_for_regular_user()
    {
        // Act
        var response = await _regularClient.GetAsync("/api/groups/all");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task get_all_members_returns_403_for_regular_user()
    {
        // Act
        var response = await _regularClient.GetAsync("/api/members");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task get_user_by_id_returns_403_for_regular_user()
    {
        // Act
        var response = await _regularClient.GetAsync("/api/members/00000000-0000-0000-0000-000000000099");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task deactivate_user_returns_403_for_regular_user()
    {
        // Act
        var response = await _regularClient.PatchAsync("/api/members/00000000-0000-0000-0000-000000000099/deactivate", null);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}

public class SuperAdminAllowedTests
{
    [Fact]
    public async Task get_all_groups_returns_200_for_super_admin()
    {
        // Arrange
        await using var factory = new TestWebApplicationFactory();
        factory.GroupService
            .Setup(service => service.GetAllGroupsAsync())
            .ReturnsAsync(new List<GroupSummaryResponse>());
        using var client = factory.CreateSuperAdminClient();

        // Act
        var response = await client.GetAsync("/api/groups/all");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

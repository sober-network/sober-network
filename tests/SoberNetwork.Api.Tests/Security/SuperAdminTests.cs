using System.Net;
using Moq;
using SoberNetwork.Api.Tests.Infrastructure;
using SoberNetwork.Core.DTOs;
using SoberNetwork.Core.DTOs.Groups;

namespace SoberNetwork.Api.Tests.Security;

public class SuperAdminForbiddenTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _regularClient;

    public SuperAdminForbiddenTests(TestWebApplicationFactory factory) => _regularClient = factory.CreateAuthenticatedClient();

    [Fact]
    public async Task GetAllGroups_AsRegularUser_ReturnsForbidden()
    {
        // Act
        var response = await _regularClient.GetAsync("/api/groups/all");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetAllMembers_AsRegularUser_ReturnsForbidden()
    {
        // Act
        var response = await _regularClient.GetAsync("/api/members");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUserById_AsRegularUser_ReturnsForbidden()
    {
        // Act
        var response = await _regularClient.GetAsync("/api/members/00000000-0000-0000-0000-000000000099");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeactivateUser_AsRegularUser_ReturnsForbidden()
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
    public async Task GetAllGroups_AsSuperAdmin_ReturnsOk()
    {
        // Arrange
        await using var factory = new TestWebApplicationFactory();
        factory.GroupService
            .Setup(service => service.GetAllGroupsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(((PagedResponse<GroupSummaryResponse>?)new PagedResponse<GroupSummaryResponse>([], 1, 10, 0), (string?)null)));
        using var client = factory.CreateSuperAdminClient();

        // Act
        var response = await client.GetAsync("/api/groups/all");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

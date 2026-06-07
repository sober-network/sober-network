using System.Net;
using System.Net.Http.Json;
using Moq;
using SoberNetwork.Api.Tests.Infrastructure;
using SoberNetwork.Core.DTOs;
using SoberNetwork.Core.DTOs.Groups;

namespace SoberNetwork.Api.Tests.Security;

/// <summary>
/// Security coverage for the meeting endpoints (scoped under /api/groups/{slug}/meetings) and
/// the authenticated client-log endpoint. Verifies the three required guarantees for every
/// protected endpoint: 401 (unauthenticated), 403 (insufficient role), and cross-group isolation (T4).
/// </summary>
public class MeetingSecurityTests : IClassFixture<TestWebApplicationFactory>
{
    private const string GroupSlug = "group-alpha";
    private static readonly Guid MeetingId = Guid.Parse("00000000-0000-0000-0000-0000000000aa");

    // A signed-in member who is NOT a GroupAdmin of group-alpha.
    private static readonly Guid NonAdminUserId = Guid.Parse("00000000-0000-0000-0000-000000000050");

    // A signed-in user who is NOT a member of group-alpha at all (different tenant).
    private static readonly Guid OutsiderUserId = Guid.Parse("00000000-0000-0000-0000-000000000099");

    private static readonly object ValidCreateBody = new
    {
        name = "Test Meeting",
        time = "19:00",
        isRecurring = true,
        daysOfWeek = new[] { 1 },
        meetingType = 1, // Online — avoids the in-person Street/City requirement
        publicJoinUrl = "https://zoom.us/j/123456789",
    };

    private static readonly object ValidUpdateBody = new { name = "Renamed Meeting" };

    private readonly TestWebApplicationFactory _factory;

    public MeetingSecurityTests(TestWebApplicationFactory factory)
    {
        _factory = factory;

        // Member view: a non-member is rejected ("not a member" -> Forbidden).
        _factory.MeetingService
            .Setup(s => s.GetGroupMeetingsAsync(GroupSlug, It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(((PagedResponse<MeetingResponse>?)null, (string?)"You are not a member of this group.")));

        // Admin-only endpoints: a non-admin caller lacks permission ("permission" -> Forbidden).
        _factory.MeetingService
            .Setup(s => s.GetAdminMeetingsAsync(GroupSlug, It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(((PagedResponse<AdminMeetingResponse>?)null, (string?)"You do not have permission to view admin meetings.")));

        _factory.MeetingService
            .Setup(s => s.CreateMeetingAsync(GroupSlug, It.IsAny<CreateMeetingRequest>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(((AdminMeetingResponse?)null, "You do not have permission to create meetings."));

        _factory.MeetingService
            .Setup(s => s.UpdateMeetingAsync(GroupSlug, It.IsAny<Guid>(), It.IsAny<UpdateMeetingRequest>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(((AdminMeetingResponse?)null, "You do not have permission to update this meeting."));

        _factory.MeetingService
            .Setup(s => s.DeleteMeetingAsync(GroupSlug, It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, "You do not have permission to delete this meeting."));
    }

    // ── 401: unauthenticated ────────────────────────────────────────────────────

    [Theory]
    [InlineData("GET", "/api/groups/group-alpha/meetings")]
    [InlineData("GET", "/api/groups/group-alpha/meetings/admin")]
    [InlineData("POST", "/api/groups/group-alpha/meetings")]
    [InlineData("PUT", "/api/groups/group-alpha/meetings/00000000-0000-0000-0000-0000000000aa")]
    [InlineData("DELETE", "/api/groups/group-alpha/meetings/00000000-0000-0000-0000-0000000000aa")]
    [InlineData("POST", "/api/client-log")]
    public async Task MeetingEndpoint_WithoutJwt_Returns401(string method, string url)
    {
        using var client = _factory.CreateAnonymousClient();
        using var request = new HttpRequestMessage(new HttpMethod(method), url);
        if (method is "POST" or "PUT")
            request.Content = JsonContent.Create(new { });

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ── 403: authenticated but insufficient role (non-admin) ────────────────────

    [Fact]
    public async Task GetAdminMeetings_AsNonAdmin_ReturnsForbidden()
    {
        using var client = _factory.CreateAuthenticatedClient(NonAdminUserId);

        var response = await client.GetAsync($"/api/groups/{GroupSlug}/meetings/admin");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateMeeting_AsNonAdmin_ReturnsForbidden()
    {
        using var client = _factory.CreateAuthenticatedClient(NonAdminUserId);

        var response = await client.PostAsJsonAsync($"/api/groups/{GroupSlug}/meetings", ValidCreateBody);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateMeeting_AsNonAdmin_ReturnsForbidden()
    {
        using var client = _factory.CreateAuthenticatedClient(NonAdminUserId);

        var response = await client.PutAsJsonAsync($"/api/groups/{GroupSlug}/meetings/{MeetingId}", ValidUpdateBody);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteMeeting_AsNonAdmin_ReturnsForbidden()
    {
        using var client = _factory.CreateAuthenticatedClient(NonAdminUserId);

        var response = await client.DeleteAsync($"/api/groups/{GroupSlug}/meetings/{MeetingId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ── Cross-group: outsider cannot reach another group's meetings (T4) ─────────

    [Fact]
    public async Task GetMeetings_AsOutsider_ReturnsForbidden()
    {
        using var client = _factory.CreateAuthenticatedClient(OutsiderUserId);

        var response = await client.GetAsync($"/api/groups/{GroupSlug}/meetings");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetAdminMeetings_AsOutsider_ReturnsForbidden()
    {
        using var client = _factory.CreateAuthenticatedClient(OutsiderUserId);

        var response = await client.GetAsync($"/api/groups/{GroupSlug}/meetings/admin");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateMeeting_AsOutsider_ReturnsForbidden()
    {
        using var client = _factory.CreateAuthenticatedClient(OutsiderUserId);

        var response = await client.PostAsJsonAsync($"/api/groups/{GroupSlug}/meetings", ValidCreateBody);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteMeeting_AsOutsider_ReturnsForbidden()
    {
        using var client = _factory.CreateAuthenticatedClient(OutsiderUserId);

        var response = await client.DeleteAsync($"/api/groups/{GroupSlug}/meetings/{MeetingId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}

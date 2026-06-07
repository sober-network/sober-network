using System.Net;
using System.Net.Http.Json;
using Moq;
using SoberNetwork.Api.Tests.Infrastructure;
using SoberNetwork.Core.DTOs;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.DTOs.Members;
using SoberNetwork.Domain.Enums;

namespace SoberNetwork.Api.Tests.Security;

public class CrossGroupAccessTests
{
    private static readonly Guid OutsiderUserId = Guid.Parse("00000000-0000-0000-0000-000000000099");
    private static readonly Guid SomeMemberId   = Guid.Parse("00000000-0000-0000-0000-000000000098");
    private const string GroupSlug = "group-alpha";

    private static TestWebApplicationFactory CreateFactory()
    {
        var factory = new TestWebApplicationFactory();

        factory.GroupService
            .Setup(service => service.GetMembersAsync(GroupSlug, OutsiderUserId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<MemberSortBy>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(((PagedResponse<MemberResponse>?)null, "You are not a member of this group."));

        factory.GroupService
            .Setup(service => service.GetJoinRequestsAsync(GroupSlug, OutsiderUserId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(((PagedResponse<JoinRequestResponse>?)null, "You do not have permission to view join requests."));

        factory.GroupService
            .Setup(service => service.GetGroupBySlugAsync(GroupSlug, OutsiderUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroupResponse?)null);

        factory.MemberService
            .Setup(service => service.GetGroupPhoneListAsync(OutsiderUserId, GroupSlug, It.IsAny<CancellationToken>()))
            .ReturnsAsync(((IReadOnlyList<PhoneListEntryResponse>?)null, "You are not a member of this group."));

        factory.MemberService
            .Setup(service => service.GetMemberInGroupContextAsync(OutsiderUserId, GroupSlug, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(((MemberDetailResponse?)null, "You are not a member of this group."));

        factory.MemberService
            .Setup(service => service.SetGroupPhoneVisibilityAsync(OutsiderUserId, GroupSlug, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, "You are not an active member of this group."));

        factory.GroupService
            .Setup(service => service.ApproveMemberAsync(GroupSlug, It.IsAny<Guid>(), OutsiderUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, "You do not have permission to approve members."));

        factory.GroupService
            .Setup(service => service.RejectMemberAsync(GroupSlug, It.IsAny<Guid>(), OutsiderUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, "You do not have permission to reject members."));

        factory.GroupService
            .Setup(service => service.RemoveMemberAsync(GroupSlug, It.IsAny<Guid>(), OutsiderUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, "You do not have permission to remove members."));

        factory.GroupService
            .Setup(service => service.ChangeRoleAsync(GroupSlug, It.IsAny<Guid>(), OutsiderUserId, It.IsAny<GroupRole>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, "You do not have permission to change roles."));

        factory.GroupService
            .Setup(service => service.ChangeMemberStatusAsync(GroupSlug, It.IsAny<Guid>(), OutsiderUserId, It.IsAny<MemberStatus>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, "You do not have permission to change member status."));

        factory.GroupService
            .Setup(service => service.ClearProbationaryStatusAsync(GroupSlug, It.IsAny<Guid>(), OutsiderUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, "You do not have permission to clear probationary status."));

        factory.GroupService
            .Setup(service => service.UpdateGroupAsync(GroupSlug, It.IsAny<UpdateGroupRequest>(), OutsiderUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(((GroupResponse?)null, "You do not have permission to update this group."));

        factory.GroupService
            .Setup(service => service.SoftDeleteGroupAsync(GroupSlug, OutsiderUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, "You do not have permission to delete this group."));

        return factory;
    }

    [Fact]
    public async Task GetMembers_AsOutsider_ReturnsForbidden()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateAuthenticatedClient(OutsiderUserId);

        var response = await client.GetAsync($"/api/groups/{GroupSlug}/members");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetJoinRequests_AsOutsider_ReturnsForbidden()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateAuthenticatedClient(OutsiderUserId);

        var response = await client.GetAsync($"/api/groups/{GroupSlug}/join-requests");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetPhoneList_AsOutsider_ReturnsForbidden()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateAuthenticatedClient(OutsiderUserId);

        var response = await client.GetAsync($"/api/groups/{GroupSlug}/phone-list");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetMemberDetail_AsOutsider_ReturnsForbidden()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateAuthenticatedClient(OutsiderUserId);

        var response = await client.GetAsync($"/api/groups/{GroupSlug}/members/{SomeMemberId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task SetPhoneVisibility_AsOutsider_ReturnsForbidden()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateAuthenticatedClient(OutsiderUserId);

        var response = await client.PatchAsJsonAsync($"/api/groups/{GroupSlug}/members/me/phone-visibility", new PhoneVisibilityRequest(true));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ApproveMember_AsNonAdmin_ReturnsForbidden()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateAuthenticatedClient(OutsiderUserId);

        var response = await client.PostAsync($"/api/groups/{GroupSlug}/members/{SomeMemberId}/approve", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task RejectMember_AsNonAdmin_ReturnsForbidden()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateAuthenticatedClient(OutsiderUserId);

        var response = await client.PostAsync($"/api/groups/{GroupSlug}/members/{SomeMemberId}/reject", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task RemoveMember_AsNonAdmin_ReturnsForbidden()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateAuthenticatedClient(OutsiderUserId);

        var response = await client.DeleteAsync($"/api/groups/{GroupSlug}/members/{SomeMemberId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ChangeMemberRole_AsNonAdmin_ReturnsForbidden()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateAuthenticatedClient(OutsiderUserId);

        var response = await client.PatchAsJsonAsync($"/api/groups/{GroupSlug}/members/{SomeMemberId}/role", new ChangeRoleRequest(GroupRole.GroupAdmin));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ChangeMemberStatus_AsNonAdmin_ReturnsForbidden()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateAuthenticatedClient(OutsiderUserId);

        var response = await client.PatchAsJsonAsync($"/api/groups/{GroupSlug}/members/{SomeMemberId}/status", new ChangeMemberStatusRequest(MemberStatus.Banned));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ClearProbation_AsNonAdmin_ReturnsForbidden()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateAuthenticatedClient(OutsiderUserId);

        var response = await client.PatchAsync($"/api/groups/{GroupSlug}/members/{SomeMemberId}/probation", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateGroup_AsNonAdmin_ReturnsForbidden()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateAuthenticatedClient(OutsiderUserId);
        var request = new UpdateGroupRequest("Hacked", null, null, null, null);

        var response = await client.PutAsJsonAsync($"/api/groups/{GroupSlug}", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteGroup_AsNonAdmin_ReturnsForbidden()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateAuthenticatedClient(OutsiderUserId);

        var response = await client.DeleteAsync($"/api/groups/{GroupSlug}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetGroup_AsOutsider_ReturnsNotFound()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateAuthenticatedClient(OutsiderUserId);

        var response = await client.GetAsync($"/api/groups/{GroupSlug}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

using SoberNetwork.Core.DTOs.Groups;

namespace SoberNetwork.Core.Interfaces;

/// <summary>Provides meeting management operations within a group. All methods enforce group_id isolation (T4).</summary>
public interface IMeetingService
{
    /// <summary>Returns all active meetings for the group, visible to authenticated members. Excludes admin-only Notes field.</summary>
    Task<(IReadOnlyList<MeetingResponse>? Meetings, string? Error)> GetGroupMeetingsAsync(string slug, string userId, CancellationToken ct = default);

    /// <summary>Returns all meetings for a group including admin Notes. Caller must be a GroupAdmin.</summary>
    Task<(IReadOnlyList<AdminMeetingResponse>? Meetings, string? Error)> GetAdminMeetingsAsync(string slug, string userId, CancellationToken ct = default);

    /// <summary>Creates a new meeting for the group. Caller must be a GroupAdmin.</summary>
    Task<(AdminMeetingResponse? Meeting, string? Error)> CreateMeetingAsync(string slug, CreateMeetingRequest request, string userId, CancellationToken ct = default);

    /// <summary>Updates mutable meeting fields. Caller must be a GroupAdmin.</summary>
    Task<(AdminMeetingResponse? Meeting, string? Error)> UpdateMeetingAsync(string slug, Guid meetingId, UpdateMeetingRequest request, string userId, CancellationToken ct = default);

    /// <summary>Soft-deletes a meeting. Caller must be a GroupAdmin.</summary>
    Task<(bool Success, string? Error)> DeleteMeetingAsync(string slug, Guid meetingId, string userId, CancellationToken ct = default);
}

using SoberNetwork.Core.DTOs;
using SoberNetwork.Core.DTOs.Groups;

namespace SoberNetwork.Core.Interfaces;

/// <summary>Provides meeting management operations within a group. All methods enforce group_id isolation (T4).</summary>
public interface IMeetingService
{
    /// <summary>Returns all active meetings for the group, visible to authenticated members. Excludes admin-only Notes field.</summary>
    Task<(PagedResponse<MeetingResponse>? Meetings, string? Error)> GetGroupMeetingsAsync(
        string slug, Guid userId, int page = 1, int pageSize = 50,
        string? search = null, MeetingSortBy sortBy = MeetingSortBy.Time,
        CancellationToken ct = default);

    /// <summary>Returns all meetings for a group including admin Notes. Caller must be a GroupAdmin.</summary>
    Task<(PagedResponse<AdminMeetingResponse>? Meetings, string? Error)> GetAdminMeetingsAsync(
        string slug, Guid userId, int page = 1, int pageSize = 50,
        string? search = null, MeetingSortBy sortBy = MeetingSortBy.Time,
        CancellationToken ct = default);

    /// <summary>Creates a new meeting for the group. Caller must be a GroupAdmin.</summary>
    Task<(AdminMeetingResponse? Meeting, string? Error)> CreateMeetingAsync(string slug, CreateMeetingRequest request, Guid userId, CancellationToken ct = default);

    /// <summary>Updates mutable meeting fields. Caller must be a GroupAdmin.</summary>
    Task<(AdminMeetingResponse? Meeting, string? Error)> UpdateMeetingAsync(string slug, Guid meetingId, UpdateMeetingRequest request, Guid userId, CancellationToken ct = default);

    /// <summary>Soft-deletes a meeting. Caller must be a GroupAdmin.</summary>
    Task<(bool Success, string? Error)> DeleteMeetingAsync(string slug, Guid meetingId, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Public cross-group meeting search. No auth required. Only searches active, publicly-listed groups (T4).
    /// Supports filtering by day, time block, format, meeting type, open/closed, and optional location radius.
    /// </summary>
    Task<IReadOnlyList<PublicMeetingSearchResponse>> SearchPublicMeetingsAsync(
        int[]? days,
        Domain.Enums.TimeBlock? timeBlock,
        string[]? formats,
        Domain.Enums.MeetingType? meetingType,
        bool? isOpen,
        double? latitude,
        double? longitude,
        double? radiusMiles,
        CancellationToken ct = default);
}


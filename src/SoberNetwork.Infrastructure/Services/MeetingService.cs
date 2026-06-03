using Microsoft.EntityFrameworkCore;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Domain.Entities;
using SoberNetwork.Domain.Enums;
using SoberNetwork.Infrastructure.Data;

namespace SoberNetwork.Infrastructure.Services;

/// <summary>
/// Meeting management service. All operations are scoped to a single group (T4 isolation).
/// Read operations require active membership. Write operations require GroupAdmin role.
/// </summary>
public sealed class MeetingService(AppDbContext db) : IMeetingService
{
    /// <inheritdoc/>
    public async Task<(IReadOnlyList<MeetingResponse>? Meetings, string? Error)> GetGroupMeetingsAsync(
        string slug, string userId, CancellationToken ct = default)
    {
        var group = await db.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Slug == slug && g.DeletedAt == null, ct);
        if (group == null) return (null, "Group not found.");

        var isMember = await db.GroupMemberships
            .AsNoTracking()
            .AnyAsync(m =>
                m.GroupId == group.Id &&
                m.UserId == userId &&
                m.Status == MemberStatus.Active &&
                m.DeletedAt == null, ct);
        if (!isMember) return (null, "You are not a member of this group.");

        var meetings = await db.Meetings
            .AsNoTracking()
            .Where(m => m.GroupId == group.Id && m.DeletedAt == null && m.IsActive)
            .OrderBy(m => m.IsRecurring ? 0 : 1).ThenBy(m => m.DayOfWeek).ThenBy(m => m.Time)
            .Select(m => new MeetingResponse(
                m.Id, m.Name, m.Description, m.IsRecurring, m.DayOfWeek, m.Time,
                m.DurationMinutes, m.OccursOn, m.IsOpen, m.Formats, m.Language,
                m.Location, m.ZoomLink, m.ZoomMeetingId, m.ZoomPasscode, m.IsActive, m.CreatedAt))
            .ToListAsync(ct);

        return (meetings, null);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<AdminMeetingResponse>? Meetings, string? Error)> GetAdminMeetingsAsync(
        string slug, string userId, CancellationToken ct = default)
    {
        var group = await db.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Slug == slug && g.DeletedAt == null, ct);
        if (group == null) return (null, "Group not found.");

        var isAdmin = await db.GroupMemberships
            .AsNoTracking()
            .AnyAsync(m =>
                m.GroupId == group.Id &&
                m.UserId == userId &&
                m.Role == GroupRole.GroupAdmin &&
                m.Status == MemberStatus.Active &&
                m.DeletedAt == null, ct);
        if (!isAdmin) return (null, "You do not have permission to view admin meetings.");

        var meetings = await db.Meetings
            .AsNoTracking()
            .Where(m => m.GroupId == group.Id && m.DeletedAt == null)
            .OrderBy(m => m.IsRecurring ? 0 : 1).ThenBy(m => m.DayOfWeek).ThenBy(m => m.Time)
            .Select(m => ToAdminResponse(m))
            .ToListAsync(ct);

        return (meetings, null);
    }

    /// <inheritdoc/>
    public async Task<(AdminMeetingResponse? Meeting, string? Error)> CreateMeetingAsync(
        string slug, CreateMeetingRequest request, string userId, CancellationToken ct = default)
    {
        var group = await db.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Slug == slug && g.DeletedAt == null, ct);
        if (group == null) return (null, "Group not found.");

        var isAdmin = await db.GroupMemberships
            .AsNoTracking()
            .AnyAsync(m =>
                m.GroupId == group.Id &&
                m.UserId == userId &&
                m.Role == GroupRole.GroupAdmin &&
                m.Status == MemberStatus.Active &&
                m.DeletedAt == null, ct);
        if (!isAdmin) return (null, "You do not have permission to create meetings.");

        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            GroupId = group.Id,
            Name = request.Name.Trim(),
            Description = NullIfWhiteSpace(request.Description),
            Notes = NullIfWhiteSpace(request.Notes),
            IsRecurring = request.IsRecurring,
            DayOfWeek = request.DayOfWeek,
            Time = request.Time.Trim(),
            DurationMinutes = request.DurationMinutes,
            OccursOn = request.OccursOn,
            IsOpen = request.IsOpen,
            Formats = NullIfWhiteSpace(request.Formats),
            Language = NullIfWhiteSpace(request.Language),
            Location = NullIfWhiteSpace(request.Location),
            ZoomLink = NullIfWhiteSpace(request.ZoomLink),
            ZoomMeetingId = NullIfWhiteSpace(request.ZoomMeetingId),
            ZoomPasscode = NullIfWhiteSpace(request.ZoomPasscode),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Meetings.Add(meeting);
        await db.SaveChangesAsync(ct);

        return (ToAdminResponse(meeting), null);
    }

    /// <inheritdoc/>
    public async Task<(AdminMeetingResponse? Meeting, string? Error)> UpdateMeetingAsync(
        string slug, Guid meetingId, UpdateMeetingRequest request, string userId, CancellationToken ct = default)
    {
        var group = await db.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Slug == slug && g.DeletedAt == null, ct);
        if (group == null) return (null, "Group not found.");

        var isAdmin = await db.GroupMemberships
            .AsNoTracking()
            .AnyAsync(m =>
                m.GroupId == group.Id &&
                m.UserId == userId &&
                m.Role == GroupRole.GroupAdmin &&
                m.Status == MemberStatus.Active &&
                m.DeletedAt == null, ct);
        if (!isAdmin) return (null, "You do not have permission to update meetings.");

        var meeting = await db.Meetings
            .FirstOrDefaultAsync(m => m.Id == meetingId && m.GroupId == group.Id && m.DeletedAt == null, ct);
        if (meeting == null) return (null, "Meeting not found.");

        if (request.Name != null) meeting.Name = request.Name.Trim();
        if (request.Description != null) meeting.Description = NullIfWhiteSpace(request.Description);
        if (request.Notes != null) meeting.Notes = NullIfWhiteSpace(request.Notes);
        if (request.IsRecurring != null) meeting.IsRecurring = request.IsRecurring.Value;
        if (request.DayOfWeek != null) meeting.DayOfWeek = request.DayOfWeek;
        if (request.Time != null) meeting.Time = request.Time.Trim();
        if (request.DurationMinutes != null) meeting.DurationMinutes = request.DurationMinutes.Value;
        if (request.OccursOn != null) meeting.OccursOn = request.OccursOn;
        if (request.IsOpen != null) meeting.IsOpen = request.IsOpen.Value;
        if (request.Formats != null) meeting.Formats = NullIfWhiteSpace(request.Formats);
        if (request.Language != null) meeting.Language = NullIfWhiteSpace(request.Language);
        if (request.Location != null) meeting.Location = NullIfWhiteSpace(request.Location);
        if (request.ZoomLink != null) meeting.ZoomLink = NullIfWhiteSpace(request.ZoomLink);
        if (request.ZoomMeetingId != null) meeting.ZoomMeetingId = NullIfWhiteSpace(request.ZoomMeetingId);
        if (request.ZoomPasscode != null) meeting.ZoomPasscode = NullIfWhiteSpace(request.ZoomPasscode);
        if (request.IsActive != null) meeting.IsActive = request.IsActive.Value;
        meeting.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return (ToAdminResponse(meeting), null);
    }

    /// <inheritdoc/>
    public async Task<(bool Success, string? Error)> DeleteMeetingAsync(
        string slug, Guid meetingId, string userId, CancellationToken ct = default)
    {
        var group = await db.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Slug == slug && g.DeletedAt == null, ct);
        if (group == null) return (false, "Group not found.");

        var isAdmin = await db.GroupMemberships
            .AsNoTracking()
            .AnyAsync(m =>
                m.GroupId == group.Id &&
                m.UserId == userId &&
                m.Role == GroupRole.GroupAdmin &&
                m.Status == MemberStatus.Active &&
                m.DeletedAt == null, ct);
        if (!isAdmin) return (false, "You do not have permission to delete meetings.");

        var meeting = await db.Meetings
            .FirstOrDefaultAsync(m => m.Id == meetingId && m.GroupId == group.Id && m.DeletedAt == null, ct);
        if (meeting == null) return (false, "Meeting not found.");

        meeting.DeletedAt = DateTime.UtcNow;
        meeting.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return (true, null);
    }

    private static AdminMeetingResponse ToAdminResponse(Meeting m) => new(
        m.Id, m.Name, m.Description, m.Notes, m.IsRecurring, m.DayOfWeek, m.Time,
        m.DurationMinutes, m.OccursOn, m.IsOpen, m.Formats, m.Language,
        m.Location, m.ZoomLink, m.ZoomMeetingId, m.ZoomPasscode, m.IsActive, m.CreatedAt, m.UpdatedAt);

    private static string? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

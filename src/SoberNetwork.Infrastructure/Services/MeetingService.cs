using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using SoberNetwork.Core.DTOs;
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
public sealed class MeetingService(AppDbContext db, ILogger<MeetingService> logger, IHttpClientFactory httpClientFactory) : IMeetingService
{
    /// <inheritdoc/>
    public async Task<(PagedResponse<MeetingResponse>? Meetings, string? Error)> GetGroupMeetingsAsync(
        string slug, Guid userId, int page = 1, int pageSize = 50,
        string? search = null, MeetingSortBy sortBy = MeetingSortBy.Time,
        CancellationToken ct = default)
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

        var query = db.Meetings
            .AsNoTracking()
            .Where(m => m.GroupId == group.Id && m.DeletedAt == null && m.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(m => m.Name.ToLower().Contains(s)
                || (m.Description != null && m.Description.ToLower().Contains(s))
                || m.Formats.Any(f => f.ToLower().Contains(s)));
        }

        query = sortBy switch
        {
            MeetingSortBy.Name => query.OrderBy(m => m.Name),
            MeetingSortBy.Type => query.OrderBy(m => m.MeetingType),
            _ => query.OrderBy(m => m.IsRecurring ? 0 : 1)
                .ThenBy(m => m.DaysOfWeek == null || m.DaysOfWeek.Length == 0 ? int.MaxValue : m.DaysOfWeek[0])
                .ThenBy(m => m.Time)
        };

        var totalCount = await query.CountAsync(ct);
        var meetings = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = meetings.Select(m => new MeetingResponse(
            m.Id, m.Name, m.Description, m.IsRecurring, m.DaysOfWeek ?? new int[] { }, m.Time,
            m.DurationMinutes, m.OccursOn, m.IsOpen, m.Formats, m.Language,
            m.MeetingType, m.VenueName, m.Location, m.Street, m.City, m.State,
            m.PostalCode, m.Country, m.Latitude, m.Longitude,
            m.ZoomLink, m.ZoomMeetingId, m.ZoomPasscode, m.PublicJoinUrl, m.IsActive, m.CreatedAt)).ToList();

        return (new PagedResponse<MeetingResponse>(items, page, pageSize, totalCount), null);
    }

    /// <inheritdoc/>
    public async Task<(PagedResponse<AdminMeetingResponse>? Meetings, string? Error)> GetAdminMeetingsAsync(
        string slug, Guid userId, int page = 1, int pageSize = 50,
        string? search = null, MeetingSortBy sortBy = MeetingSortBy.Time,
        CancellationToken ct = default)
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

        var query = db.Meetings
            .AsNoTracking()
            .Where(m => m.GroupId == group.Id && m.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(m => m.Name.ToLower().Contains(s)
                || (m.Description != null && m.Description.ToLower().Contains(s))
                || m.Formats.Any(f => f.ToLower().Contains(s)));
        }

        query = sortBy switch
        {
            MeetingSortBy.Name => query.OrderBy(m => m.Name),
            MeetingSortBy.Type => query.OrderBy(m => m.MeetingType),
            _ => query.OrderBy(m => m.IsRecurring ? 0 : 1)
                .ThenBy(m => m.DaysOfWeek == null || m.DaysOfWeek.Length == 0 ? int.MaxValue : m.DaysOfWeek[0])
                .ThenBy(m => m.Time)
        };

        var totalCount = await query.CountAsync(ct);
        var meetings = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (new PagedResponse<AdminMeetingResponse>(
            meetings.Select(ToAdminResponse).ToList(), page, pageSize, totalCount), null);
    }

    /// <inheritdoc/>
    public async Task<(AdminMeetingResponse? Meeting, string? Error)> CreateMeetingAsync(
        string slug, CreateMeetingRequest request, Guid userId, CancellationToken ct = default)
    {
        logger.LogInformation("CreateMeetingAsync: slug={Slug}, userId={UserId}", slug, userId);
         
        var group = await db.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Slug == slug && g.DeletedAt == null, ct);
        if (group == null)
        {
            logger.LogWarning("CreateMeetingAsync: Group not found. slug={Slug}", slug);
            return (null, "Group not found.");
        }

        var isAdmin = await db.GroupMemberships
            .AsNoTracking()
            .AnyAsync(m =>
                m.GroupId == group.Id &&
                m.UserId == userId &&
                m.Role == GroupRole.GroupAdmin &&
                m.Status == MemberStatus.Active &&
                m.DeletedAt == null, ct);
        if (!isAdmin)
        {
            logger.LogWarning("CreateMeetingAsync: User not admin. userId={UserId}, groupId={GroupId}", userId, group.Id);
            return (null, "You do not have permission to create meetings.");
        }

        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            GroupId = group.Id,
            Name = request.Name.Trim(),
            Description = NullIfWhiteSpace(request.Description),
            Notes = NullIfWhiteSpace(request.Notes),
            IsRecurring = request.IsRecurring,
            DaysOfWeek = request.DaysOfWeek?.ToArray(),
            Time = request.Time.Trim(),
            DurationMinutes = request.DurationMinutes,
            OccursOn = request.OccursOn,
            IsOpen = request.IsOpen,
            Formats = request.Formats?.ToList() ?? [],
            Language = NullIfWhiteSpace(request.Language),
            MeetingType = request.MeetingType,
            VenueName = NullIfWhiteSpace(request.VenueName),
            Location = NullIfWhiteSpace(request.Location),
            Street = NullIfWhiteSpace(request.Street),
            City = NullIfWhiteSpace(request.City),
            State = NullIfWhiteSpace(request.State),
            PostalCode = NullIfWhiteSpace(request.PostalCode),
            Country = NullIfWhiteSpace(request.Country),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            ZoomLink = NullIfWhiteSpace(request.ZoomLink),
            ZoomMeetingId = NullIfWhiteSpace(request.ZoomMeetingId),
            ZoomPasscode = NullIfWhiteSpace(request.ZoomPasscode),
            PublicJoinUrl = NullIfWhiteSpace(request.PublicJoinUrl),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Auto-geocode when address is provided and coordinates were not supplied explicitly.
        if (meeting.Latitude == null && meeting.Longitude == null)
            await GeocodeAsync(meeting, ct);

        try
        {
            db.Meetings.Add(meeting);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("CreateMeetingAsync: Success. meetingId={MeetingId}", meeting.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CreateMeetingAsync: SaveChangesAsync failed. meetingId={MeetingId}, groupId={GroupId}", meeting.Id, group.Id);
            return (null, $"Failed to save meeting: {ex.Message}");
        }

        return (ToAdminResponse(meeting), null);
    }

    /// <inheritdoc/>
    public async Task<(AdminMeetingResponse? Meeting, string? Error)> UpdateMeetingAsync(
        string slug, Guid meetingId, UpdateMeetingRequest request, Guid userId, CancellationToken ct = default)
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
        if (request.DaysOfWeek != null) meeting.DaysOfWeek = request.DaysOfWeek.ToArray();
        if (request.Time != null) meeting.Time = request.Time.Trim();
        if (request.DurationMinutes != null) meeting.DurationMinutes = request.DurationMinutes.Value;
        if (request.OccursOn != null) meeting.OccursOn = request.OccursOn;
        if (request.IsOpen != null) meeting.IsOpen = request.IsOpen.Value;
        if (request.Formats != null) meeting.Formats = request.Formats.ToList();
        if (request.Language != null) meeting.Language = NullIfWhiteSpace(request.Language);
        if (request.MeetingType != null) meeting.MeetingType = request.MeetingType.Value;
        if (request.VenueName != null) meeting.VenueName = NullIfWhiteSpace(request.VenueName);
        if (request.Location != null) meeting.Location = NullIfWhiteSpace(request.Location);
        if (request.Street != null) meeting.Street = NullIfWhiteSpace(request.Street);
        if (request.City != null) meeting.City = NullIfWhiteSpace(request.City);
        if (request.State != null) meeting.State = NullIfWhiteSpace(request.State);
        if (request.PostalCode != null) meeting.PostalCode = NullIfWhiteSpace(request.PostalCode);
        if (request.Country != null) meeting.Country = NullIfWhiteSpace(request.Country);
        if (request.Latitude != null) meeting.Latitude = request.Latitude;
        if (request.Longitude != null) meeting.Longitude = request.Longitude;
        if (request.ZoomLink != null) meeting.ZoomLink = NullIfWhiteSpace(request.ZoomLink);
        if (request.ZoomMeetingId != null) meeting.ZoomMeetingId = NullIfWhiteSpace(request.ZoomMeetingId);
        if (request.ZoomPasscode != null) meeting.ZoomPasscode = NullIfWhiteSpace(request.ZoomPasscode);
        if (request.PublicJoinUrl != null) meeting.PublicJoinUrl = NullIfWhiteSpace(request.PublicJoinUrl);
        if (request.IsActive != null) meeting.IsActive = request.IsActive.Value;

        // Re-geocode when any address field changed and the caller didn't supply explicit coordinates.
        bool addressChanged = request.Street != null || request.City != null || request.State != null
            || request.PostalCode != null || request.Country != null;
        if (addressChanged && request.Latitude == null && request.Longitude == null)
            await GeocodeAsync(meeting, ct);

        meeting.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return (ToAdminResponse(meeting), null);
    }

    /// <inheritdoc/>
    public async Task<(bool Success, string? Error)> DeleteMeetingAsync(
        string slug, Guid meetingId, Guid userId, CancellationToken ct = default)
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
        m.Id, m.Name, m.Description, m.Notes, m.IsRecurring, m.DaysOfWeek ?? [], m.Time,
        m.DurationMinutes, m.OccursOn, m.IsOpen, m.Formats, m.Language,
        m.MeetingType, m.VenueName, m.Location, m.Street, m.City, m.State,
        m.PostalCode, m.Country, m.Latitude, m.Longitude,
        m.ZoomLink, m.ZoomMeetingId, m.ZoomPasscode, m.PublicJoinUrl, m.IsActive, m.CreatedAt, m.UpdatedAt);

    private static string? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    /// <summary>
    /// Best-effort geocoding via Nominatim. Builds a query from the meeting's address fields
    /// and populates Latitude/Longitude. Silently no-ops if the address is empty or the request fails.
    /// </summary>
    private async Task GeocodeAsync(Meeting meeting, CancellationToken ct)
    {
        var parts = new[] { meeting.Street, meeting.City, meeting.State, meeting.PostalCode, meeting.Country };
        var query = string.Join(", ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
        if (string.IsNullOrWhiteSpace(query)) return;

        meeting.Latitude = null;
        meeting.Longitude = null;

        try
        {
            var http = httpClientFactory.CreateClient("Nominatim");
            var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(query)}&format=json&limit=1";
            var results = await http.GetFromJsonAsync<NominatimResult[]>(url, ct);
            if (results is { Length: > 0 })
            {
                meeting.Latitude  = results[0].Lat;
                meeting.Longitude = results[0].Lon;
                return;
            }

            // Street-level lookup failed — retry with city/state only for a coarser pin.
            var fallbackParts = new[] { meeting.City, meeting.State, meeting.PostalCode };
            var fallback = string.Join(", ", fallbackParts.Where(p => !string.IsNullOrWhiteSpace(p)));
            if (!string.IsNullOrWhiteSpace(fallback))
            {
                var fallbackUrl = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(fallback)}&format=json&limit=1";
                var fallbackResults = await http.GetFromJsonAsync<NominatimResult[]>(fallbackUrl, ct);
                if (fallbackResults is { Length: > 0 })
                {
                    meeting.Latitude  = fallbackResults[0].Lat;
                    meeting.Longitude = fallbackResults[0].Lon;
                }
            }
        }
        catch
        {
            // Geocoding is best-effort — a failure must not block the meeting save.
        }
    }

    private sealed class NominatimResult
    {
        public double Lat { get; init; }
        public double Lon { get; init; }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PublicMeetingSearchResponse>> SearchPublicMeetingsAsync(
        int[]? days,
        TimeBlock? timeBlock,
        string[]? formats,
        MeetingType? meetingType,
        bool? isOpen,
        double? latitude,
        double? longitude,
        double? radiusMiles,
        CancellationToken ct = default)
    {
        var radius = radiusMiles ?? 25.0;

        var query = db.Meetings
            .AsNoTracking()
            .Include(m => m.Group)
            .Where(m =>
                m.DeletedAt == null &&
                m.IsActive &&
                m.Group.DeletedAt == null &&
                m.Group.IsActive &&
                m.Group.IsPublic);

        if (days?.Length > 0)
            query = query.Where(m => m.DaysOfWeek != null && m.DaysOfWeek.Any(d => days.Contains(d)));

        if (timeBlock.HasValue)
        {
            query = timeBlock.Value switch
            {
                TimeBlock.Morning   => query.Where(m => string.Compare(m.Time, "06:00") >= 0 && string.Compare(m.Time, "12:00") < 0),
                TimeBlock.Afternoon => query.Where(m => string.Compare(m.Time, "12:00") >= 0 && string.Compare(m.Time, "17:00") < 0),
                TimeBlock.Evening   => query.Where(m => string.Compare(m.Time, "17:00") >= 0 && string.Compare(m.Time, "21:00") < 0),
                TimeBlock.Night     => query.Where(m => string.Compare(m.Time, "21:00") >= 0 || string.Compare(m.Time, "06:00") < 0),
                _ => query
            };
        }

        if (formats?.Length > 0)
            query = query.Where(m => m.Formats.Any(f => formats.Contains(f)));

        if (meetingType.HasValue)
            query = query.Where(m => m.MeetingType == meetingType.Value);

        if (isOpen.HasValue)
            query = query.Where(m => m.IsOpen == isOpen.Value);

        // Apply location radius filter when lat/lon provided.
        // Meetings with coordinates use their own; online meetings with no coordinates fall back
        // to the group's district lat/lon so they still appear in location-based searches.
        if (latitude.HasValue && longitude.HasValue)
        {
            var lat = latitude.Value;
            var lon = longitude.Value;
            // Haversine approximation: 1 degree latitude ≈ 69 miles; 1 degree longitude ≈ 69 * cos(lat) miles
            var latDelta = radius / 69.0;
            var lonDelta = radius / (69.0 * Math.Cos(lat * Math.PI / 180.0));

            query = query.Where(m =>
                // Meeting has its own coordinates — use them
                (m.Latitude != null && m.Longitude != null &&
                 m.Latitude >= lat - latDelta && m.Latitude <= lat + latDelta &&
                 m.Longitude >= lon - lonDelta && m.Longitude <= lon + lonDelta)
                ||
                // Meeting has no coordinates — fall back to group's district location
                (m.Latitude == null && m.Longitude == null &&
                 m.Group.DistrictLatitude != null && m.Group.DistrictLongitude != null &&
                 m.Group.DistrictLatitude >= lat - latDelta && m.Group.DistrictLatitude <= lat + latDelta &&
                 m.Group.DistrictLongitude >= lon - lonDelta && m.Group.DistrictLongitude <= lon + lonDelta));
        }

        var meetings = await query
            .OrderBy(m => m.DaysOfWeek == null || m.DaysOfWeek.Length == 0 ? int.MaxValue : m.DaysOfWeek[0])       
            .ThenBy(m => m.Time)
            .ThenBy(m => m.Name)
            .ToListAsync(ct);

        return meetings.Select(m =>
        {
            // Use meeting coords if available, otherwise fall back to group district coords
            var effectiveLat = m.Latitude ?? m.Group?.DistrictLatitude;
            var effectiveLon = m.Longitude ?? m.Group?.DistrictLongitude;

            double? distanceMiles = null;
            if (latitude.HasValue && longitude.HasValue && effectiveLat.HasValue && effectiveLon.HasValue)
                distanceMiles = HaversineDistance(latitude.Value, longitude.Value, effectiveLat.Value, effectiveLon.Value);

            return new PublicMeetingSearchResponse(
                m.Id, m.Name, m.Description,
                m.Group.Name, m.Group.Slug,
                m.MeetingType, m.IsRecurring, m.DaysOfWeek ?? new int[] { }, m.Time, m.DurationMinutes, m.OccursOn,
                m.IsOpen, m.Formats, m.Language,
                m.VenueName, m.Location, m.Street, m.City, m.State, m.PostalCode, m.Country,
                m.Latitude, m.Longitude, m.PublicJoinUrl, distanceMiles);
        }).ToList();
    }

    private static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 3958.8; // Earth radius in miles
        var dLat = (lat2 - lat1) * Math.PI / 180.0;
        var dLon = (lon2 - lon1) * Math.PI / 180.0;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}

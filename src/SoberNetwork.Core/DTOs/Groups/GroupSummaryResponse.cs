namespace SoberNetwork.Core.DTOs.Groups;

/// <summary>Minimal group information safe for discovery and join screens.</summary>
public record GroupSummaryResponse(
    /// <summary>Display name of the group.</summary>
    string Name,
    /// <summary>Permanent URL slug for the group.</summary>
    string Slug,
    /// <summary>Optional group description.</summary>
    string? Description,
    /// <summary>Optional time zone label.</summary>
    string? TimeZone,
    /// <summary>Whether the group is currently active.</summary>
    bool IsActive,
    /// <summary>Whether the group is listed in public discovery views.</summary>
    bool IsPublic,
    /// <summary>Whether join requests require admin approval.</summary>
    bool RequiresApproval,
    /// <summary>
    /// Publicly-safe meeting information.
    /// T11/T12 reviewed: time/day/location are non-member data equivalent to AA Intergroup listings.
    /// Zoom credentials excluded from this response.
    /// </summary>
    IReadOnlyList<PublicMeetingResponse> Meetings
);

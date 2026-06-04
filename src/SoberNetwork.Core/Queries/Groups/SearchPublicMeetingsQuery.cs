using MediatR;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Domain.Enums;

namespace SoberNetwork.Core.Queries.Groups;

/// <summary>
/// Query for the public cross-group meeting finder.
/// No authentication required. Only returns meetings from active, publicly-listed groups (IsPublic=true, T4).
/// </summary>
public record SearchPublicMeetingsQuery(
    /// <summary>Filter by days of week (0=Sun..6=Sat). Null = all days.</summary>
    int[]? Days,
    /// <summary>Filter by time of day bucket. Null = any time.</summary>
    TimeBlock? TimeBlock,
    /// <summary>Filter by one or more meeting formats. Null = all formats.</summary>
    string[]? Formats,
    /// <summary>Filter by meeting type. Null = all types.</summary>
    MeetingType? MeetingType,
    /// <summary>Filter by open/closed status. Null = both.</summary>
    bool? IsOpen,
    /// <summary>Origin latitude for distance-based search. Null = no distance filter.</summary>
    double? Latitude,
    /// <summary>Origin longitude for distance-based search. Null = no distance filter.</summary>
    double? Longitude,
    /// <summary>Search radius in miles. Ignored when Lat/Lon are null. Defaults to 25.</summary>
    double? RadiusMiles
) : IRequest<IReadOnlyList<PublicMeetingSearchResponse>>;

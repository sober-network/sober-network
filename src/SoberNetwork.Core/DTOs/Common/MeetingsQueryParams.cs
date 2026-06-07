using SoberNetwork.Core.DTOs.Groups;

namespace SoberNetwork.Core.DTOs.Common;

/// <summary>Query parameters for the meetings list endpoint.</summary>
public record MeetingsQueryParams(
    int Page = 1,
    int PageSize = 50,
    string? Search = null,
    MeetingSortBy SortBy = MeetingSortBy.Time
);

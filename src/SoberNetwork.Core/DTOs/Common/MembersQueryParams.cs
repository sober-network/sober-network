using SoberNetwork.Core.DTOs.Groups;

namespace SoberNetwork.Core.DTOs.Common;

/// <summary>Query parameters for the group member list endpoint.</summary>
public record MembersQueryParams(
    int Page = 1,
    int PageSize = 25,
    string? Search = null,
    MemberSortBy SortBy = MemberSortBy.Name,
    bool SortDescending = false
);

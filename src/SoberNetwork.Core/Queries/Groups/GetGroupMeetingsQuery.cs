using MediatR;
using SoberNetwork.Core.DTOs;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Queries.Groups;

/// <summary>Returns meetings for a group visible to authenticated members (no admin Notes).</summary>
public record GetGroupMeetingsQuery(
    string Slug,
    Guid UserId,
    int Page = 1,
    int PageSize = 50,
    string? Search = null,
    MeetingSortBy SortBy = MeetingSortBy.Time
) : IRequest<DataResult<PagedResponse<MeetingResponse>>>;

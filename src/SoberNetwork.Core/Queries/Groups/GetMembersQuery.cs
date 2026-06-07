using MediatR;
using SoberNetwork.Core.DTOs;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Queries.Groups;

/// <summary>Returns a paged, searchable, sortable list of members for a group.</summary>
public record GetMembersQuery(
    string Slug,
    Guid UserId,
    int Page,
    int PageSize,
    string? Search = null,
    MemberSortBy SortBy = MemberSortBy.Name,
    bool SortDescending = false
) : IRequest<DataResult<PagedResponse<MemberResponse>>>;

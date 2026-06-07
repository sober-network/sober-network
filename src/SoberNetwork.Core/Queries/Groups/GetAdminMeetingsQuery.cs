using MediatR;
using SoberNetwork.Core.DTOs;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Queries.Groups;

/// <summary>Returns all meetings for a group including admin-only Notes. Caller must be a GroupAdmin.</summary>
public record GetAdminMeetingsQuery(string Slug, Guid UserId, int Page = 1, int PageSize = 10) : IRequest<DataResult<PagedResponse<AdminMeetingResponse>>>;

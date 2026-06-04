using MediatR;
using SoberNetwork.Core.DTOs;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Queries.Groups;

public record GetJoinRequestsQuery(string Slug, Guid UserId, int Page, int PageSize) : IRequest<DataResult<PagedResponse<JoinRequestResponse>>>;

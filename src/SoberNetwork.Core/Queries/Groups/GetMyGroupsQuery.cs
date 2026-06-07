using MediatR;
using SoberNetwork.Core.DTOs;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Queries.Groups;

public record GetMyGroupsQuery(Guid UserId, int Page = 1, int PageSize = 10) : IRequest<DataResult<PagedResponse<GroupResponse>>>;

using MediatR;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Queries.Groups;

public record GetMyGroupsQuery(Guid UserId) : IRequest<IReadOnlyList<GroupResponse>>;

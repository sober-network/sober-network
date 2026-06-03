using MediatR;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Queries.Groups;

public record GetMyGroupsQuery(string UserId) : IRequest<IReadOnlyList<GroupResponse>>;

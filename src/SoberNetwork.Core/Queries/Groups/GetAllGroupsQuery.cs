using MediatR;
using SoberNetwork.Core.DTOs.Groups;

namespace SoberNetwork.Core.Queries.Groups;

public record GetAllGroupsQuery : IRequest<IReadOnlyList<GroupSummaryResponse>>;

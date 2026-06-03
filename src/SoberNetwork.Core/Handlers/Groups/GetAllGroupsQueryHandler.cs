using MediatR;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Queries.Groups;

namespace SoberNetwork.Core.Handlers.Groups;

public class GetAllGroupsQueryHandler(IGroupService groupService) : IRequestHandler<GetAllGroupsQuery, IReadOnlyList<GroupSummaryResponse>>
{
    public Task<IReadOnlyList<GroupSummaryResponse>> Handle(GetAllGroupsQuery request, CancellationToken cancellationToken) =>
        groupService.GetAllGroupsAsync();
}

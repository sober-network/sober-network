using MediatR;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Queries.Groups;

namespace SoberNetwork.Core.Handlers.Groups;

public class GetMyGroupsQueryHandler(IGroupService groupService) : IRequestHandler<GetMyGroupsQuery, IReadOnlyList<GroupResponse>>
{
    public Task<IReadOnlyList<GroupResponse>> Handle(GetMyGroupsQuery request, CancellationToken cancellationToken) =>
        groupService.GetUserGroupsAsync(request.UserId);
}

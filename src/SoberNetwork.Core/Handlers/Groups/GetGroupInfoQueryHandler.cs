using MediatR;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Queries.Groups;

namespace SoberNetwork.Core.Handlers.Groups;

public class GetGroupInfoQueryHandler(IGroupService groupService) : IRequestHandler<GetGroupInfoQuery, GroupSummaryResponse?>
{
    public Task<GroupSummaryResponse?> Handle(GetGroupInfoQuery request, CancellationToken cancellationToken) =>
        groupService.GetGroupInfoAsync(request.Slug, cancellationToken);
}

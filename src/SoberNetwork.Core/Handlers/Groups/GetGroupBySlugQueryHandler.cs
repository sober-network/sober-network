using MediatR;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Queries.Groups;

namespace SoberNetwork.Core.Handlers.Groups;

public class GetGroupBySlugQueryHandler(IGroupService groupService) : IRequestHandler<GetGroupBySlugQuery, GroupResponse?>
{
    public Task<GroupResponse?> Handle(GetGroupBySlugQuery request, CancellationToken cancellationToken) =>
        groupService.GetGroupBySlugAsync(request.Slug, request.UserId, cancellationToken);
}

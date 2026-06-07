using MediatR;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Queries.Groups;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Groups;

/// <summary>Handles retrieval of group service-role assignments.</summary>
public class GetGroupServiceRolesQueryHandler(IGroupServiceRoleService service)
    : IRequestHandler<GetGroupServiceRolesQuery, DataResult<IReadOnlyList<GroupServiceRoleResponse>>>
{
    /// <inheritdoc />
    public async Task<DataResult<IReadOnlyList<GroupServiceRoleResponse>>> Handle(
        GetGroupServiceRolesQuery request, CancellationToken cancellationToken)
    {
        var (roles, error) = await service.GetRolesAsync(request.Slug, request.UserId, cancellationToken);
        if (error is not null)
            return DataResult<IReadOnlyList<GroupServiceRoleResponse>>.Fail(
                error.Contains("not a member") ? ResultCode.Forbidden : ResultCode.NotFound, error);
        return DataResult<IReadOnlyList<GroupServiceRoleResponse>>.Ok(roles!);
    }
}

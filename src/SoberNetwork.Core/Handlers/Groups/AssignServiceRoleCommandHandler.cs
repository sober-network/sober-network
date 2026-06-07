using MediatR;
using SoberNetwork.Core.Commands.Groups;
using SoberNetwork.Core.DTOs.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Groups;

/// <summary>Handles assignment of service roles within a group.</summary>
public class AssignServiceRoleCommandHandler(IGroupServiceRoleService service)
    : IRequestHandler<AssignServiceRoleCommand, DataResult<GroupServiceRoleResponse>>
{
    /// <inheritdoc />
    public async Task<DataResult<GroupServiceRoleResponse>> Handle(
        AssignServiceRoleCommand request, CancellationToken cancellationToken)
    {
        var (role, error) = await service.AssignRoleAsync(request.Slug, request.Request, request.AdminUserId, cancellationToken);
        if (error is not null)
            return DataResult<GroupServiceRoleResponse>.Fail(
                error.Contains("permission") ? ResultCode.Forbidden : ResultCode.BadRequest, error);
        return DataResult<GroupServiceRoleResponse>.Ok(role!);
    }
}

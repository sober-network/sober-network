using MediatR;
using SoberNetwork.Core.Commands.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Groups;

/// <summary>Handles removal of group service-role assignments.</summary>
public class RemoveServiceRoleCommandHandler(IGroupServiceRoleService service)
    : IRequestHandler<RemoveServiceRoleCommand, CommandResult>
{
    /// <inheritdoc />
    public async Task<CommandResult> Handle(RemoveServiceRoleCommand request, CancellationToken cancellationToken)
    {
        var (success, error) = await service.RemoveRoleAsync(request.Slug, request.RoleId, request.AdminUserId, cancellationToken);
        if (!success)
            return CommandResult.Fail(error!.Contains("permission") ? ResultCode.Forbidden : ResultCode.NotFound, error!);
        return CommandResult.Ok();
    }
}

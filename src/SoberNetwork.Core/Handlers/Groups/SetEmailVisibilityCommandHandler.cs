using MediatR;
using SoberNetwork.Core.Commands.Groups;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Groups;

/// <summary>Handles updates to the caller's per-group email sharing preference.</summary>
public class SetEmailVisibilityCommandHandler(IMemberService memberService) : IRequestHandler<SetEmailVisibilityCommand, CommandResult>
{
    /// <inheritdoc />
    public async Task<CommandResult> Handle(SetEmailVisibilityCommand request, CancellationToken cancellationToken)
    {
        var (success, error) = await memberService.SetGroupEmailVisibilityAsync(request.UserId, request.Slug, request.IsShared, cancellationToken);
        if (!success)
            return CommandResult.Fail(error!.Contains("not an active member") ? ResultCode.Forbidden : ResultCode.BadRequest, error!);
        return CommandResult.Ok();
    }
}

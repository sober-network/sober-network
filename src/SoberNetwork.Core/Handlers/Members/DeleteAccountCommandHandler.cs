using MediatR;
using SoberNetwork.Core.Commands.Members;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Members;

public class DeleteAccountCommandHandler(IMemberService memberService) : IRequestHandler<DeleteAccountCommand, CommandResult>
{
    public async Task<CommandResult> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var (success, error) = await memberService.DeleteAccountAsync(request.UserId, request.Password);
        if (!success)
            return CommandResult.Fail(error!.Contains("Incorrect") ? ResultCode.Unauthorized : ResultCode.BadRequest, error!);
        return CommandResult.Ok();
    }
}

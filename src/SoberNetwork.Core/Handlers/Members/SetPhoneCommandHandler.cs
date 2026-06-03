using MediatR;
using SoberNetwork.Core.Commands.Members;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Members;

public class SetPhoneCommandHandler(IMemberService memberService) : IRequestHandler<SetPhoneCommand, CommandResult>
{
    public async Task<CommandResult> Handle(SetPhoneCommand request, CancellationToken cancellationToken)
    {
        var (success, error) = await memberService.SetPhoneAsync(request.UserId, request.PhoneNumber);
        if (!success)
            return CommandResult.Fail(ResultCode.BadRequest, error!);
        return CommandResult.Ok();
    }
}

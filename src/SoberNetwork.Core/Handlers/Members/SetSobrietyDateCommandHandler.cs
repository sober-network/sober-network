using MediatR;
using SoberNetwork.Core.Commands.Members;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Members;

public class SetSobrietyDateCommandHandler(IMemberService memberService) : IRequestHandler<SetSobrietyDateCommand, CommandResult>
{
    public async Task<CommandResult> Handle(SetSobrietyDateCommand request, CancellationToken cancellationToken)
    {
        var (success, error) = await memberService.SetSobrietyDateAsync(request.UserId, request.SobrietyDate);
        if (!success)
            return CommandResult.Fail(ResultCode.BadRequest, error!);
        return CommandResult.Ok();
    }
}

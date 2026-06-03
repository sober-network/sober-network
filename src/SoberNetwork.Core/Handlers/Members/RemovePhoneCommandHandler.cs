using MediatR;
using SoberNetwork.Core.Commands.Members;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Members;

public class RemovePhoneCommandHandler(IMemberService memberService) : IRequestHandler<RemovePhoneCommand, CommandResult>
{
    public async Task<CommandResult> Handle(RemovePhoneCommand request, CancellationToken cancellationToken)
    {
        var success = await memberService.RemovePhoneAsync(request.UserId);
        return success ? CommandResult.Ok() : CommandResult.Fail(ResultCode.NotFound, "No phone number on record.");
    }
}

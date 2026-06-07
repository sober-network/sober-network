using MediatR;
using SoberNetwork.Core.Commands.Members;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Members;

public class RemoveSobrietyDateCommandHandler(IMemberService memberService) : IRequestHandler<RemoveSobrietyDateCommand, CommandResult>
{
    public async Task<CommandResult> Handle(RemoveSobrietyDateCommand request, CancellationToken cancellationToken)
    {
        var success = await memberService.RemoveSobrietyDateAsync(request.UserId, cancellationToken);
        return success ? CommandResult.Ok() : CommandResult.Fail(ResultCode.NotFound, "No sobriety date on record.");
    }
}

using MediatR;
using SoberNetwork.Core.Commands.Members;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Members;

public class UpdateSobrietyVisibilityCommandHandler(IMemberService memberService) : IRequestHandler<UpdateSobrietyVisibilityCommand, CommandResult>
{
    public async Task<CommandResult> Handle(UpdateSobrietyVisibilityCommand request, CancellationToken cancellationToken)
    {
        var success = await memberService.UpdateSobrietyVisibilityAsync(request.UserId, request.IsDatePublic, request.IsDaysPublic);
        return success ? CommandResult.Ok() : CommandResult.Fail(ResultCode.NotFound, "User not found.");
    }
}

using MediatR;
using SoberNetwork.Core.Commands.Members;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.Members;

public class UpdateMailingAddressCommandHandler(IMemberService memberService)
    : IRequestHandler<UpdateMailingAddressCommand, CommandResult>
{
    public async Task<CommandResult> Handle(UpdateMailingAddressCommand request, CancellationToken cancellationToken)
    {
        var (success, error) = await memberService.UpdateMailingAddressAsync(request.UserId, request.Request);
        return success ? CommandResult.Ok() : CommandResult.Fail(ResultCode.NotFound, error!);
    }
}

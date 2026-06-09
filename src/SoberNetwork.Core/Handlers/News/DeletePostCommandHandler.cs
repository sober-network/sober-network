using MediatR;
using SoberNetwork.Core.Commands.News;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.News;

public class DeletePostCommandHandler(INewsService newsService) : IRequestHandler<DeletePostCommand, CommandResult>
{
    public async Task<CommandResult> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        var (success, error) = await newsService.DeletePostAsync(request.UserId, request.PostId, cancellationToken);
        if (!success)
        {
            var code = error!.Contains("author") || error.Contains("permission") ? ResultCode.Forbidden
                : error.Contains("not found") ? ResultCode.NotFound
                : ResultCode.BadRequest;
            return CommandResult.Fail(code, error!);
        }
        return CommandResult.Ok();
    }
}

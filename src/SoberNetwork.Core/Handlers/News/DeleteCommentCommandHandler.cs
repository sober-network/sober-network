using MediatR;
using SoberNetwork.Core.Commands.News;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Core.Handlers.News;

public class DeleteCommentCommandHandler(INewsService newsService) : IRequestHandler<DeleteCommentCommand, CommandResult>
{
    public async Task<CommandResult> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var (success, error) = await newsService.DeleteCommentAsync(request.CommentId, request.RequestingUserId, cancellationToken);
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

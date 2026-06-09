using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoberNetwork.Core.Commands.News;
using SoberNetwork.Core.DTOs.News;
using SoberNetwork.Core.Queries.News;
using SoberNetwork.Core.Results;

namespace SoberNetwork.Api.Controllers;

/// <summary>Platform-wide news and announcements feed.</summary>
[ApiController]
[Route("api/news")]
[Authorize]
public class NewsController(IMediator mediator) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Returns the news feed for the authenticated user (posts from all their groups, newest first).</summary>
    [HttpGet]
    public async Task<IActionResult> GetFeed(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetNewsFeedQuery(UserId, page, pageSize), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => Ok(result.Data),
            _ => Problem(result.Error, statusCode: 400)
        };
    }

    /// <summary>Creates a post in the specified group.</summary>
    [HttpPost]
    public async Task<IActionResult> CreatePost(
        [FromBody] CreatePostRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new CreatePostCommand(UserId, request), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => Ok(result.Data),
            ResultCode.Forbidden => Forbid(),
            ResultCode.NotFound => NotFound(new { message = result.Error }),
            _ => Problem(result.Error, statusCode: 400)
        };
    }

    /// <summary>Updates a post. Caller must be the author.</summary>
    [HttpPut("{postId:guid}")]
    public async Task<IActionResult> UpdatePost(
        Guid postId, [FromBody] UpdatePostRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new UpdatePostCommand(UserId, postId, request), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => Ok(result.Data),
            ResultCode.Forbidden => Forbid(),
            ResultCode.NotFound => NotFound(new { message = result.Error }),
            _ => Problem(result.Error, statusCode: 400)
        };
    }

    /// <summary>Deletes a post. Caller must be the author or a group admin.</summary>
    [HttpDelete("{postId:guid}")]
    public async Task<IActionResult> DeletePost(
        Guid postId,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new DeletePostCommand(UserId, postId), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => Ok(new { message = "Post deleted." }),
            ResultCode.Forbidden => Forbid(),
            ResultCode.NotFound => NotFound(new { message = result.Error }),
            _ => Problem(result.Error, statusCode: 400)
        };
    }

    /// <summary>Approves a pending post. Caller must be a group admin or superadmin.</summary>
    [HttpPost("{postId:guid}/approve")]
    public async Task<IActionResult> ApprovePost(
        Guid postId,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new ApprovePostCommand(UserId, postId), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => Ok(new { message = "Post approved." }),
            ResultCode.Forbidden => Forbid(),
            ResultCode.NotFound => NotFound(new { message = result.Error }),
            _ => Problem(result.Error, statusCode: 400)
        };
    }
}

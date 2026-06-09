using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoberNetwork.Core.Commands.News;
using SoberNetwork.Core.DTOs.News;
using SoberNetwork.Core.Queries.News;
using SoberNetwork.Core.Results;
using SoberNetwork.Infrastructure.Data;

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

    // ── Media uploads ────────────────────────────────────────────────────────────

    /// <summary>Uploads media (image or video) for a post. Returns media ID and preview URL.</summary>
    [HttpPost("upload")]
    [RequestSizeLimit(52428800)]  // 50 MB limit for video uploads
    public async Task<IActionResult> UploadMedia(
        [FromQuery] string groupSlug,
        CancellationToken cancellationToken = default)
    {
        // Verify file is uploaded
        var file = Request.Form.Files.FirstOrDefault();
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded." });

        try
        {
            // Upload via MediatR command (which will validate group access)
            // Get app db context to fetch group ID
            var db = HttpContext.RequestServices.GetRequiredService<SoberNetwork.Infrastructure.Data.AppDbContext>();
            var group = await db.Groups
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Slug == groupSlug && g.DeletedAt == null, cancellationToken);

            if (group == null)
                return NotFound(new { message = "Group not found." });

            using var stream = file.OpenReadStream();
            var command = new UploadPostMediaCommand(
                group.Id,
                stream,
                file.FileName,
                file.ContentType);

            var result = await mediator.Send(command, cancellationToken);

            return Ok(new
            {
                mediaId = result.MediaId,
                mediaUrl = result.MediaUrl,
                thumbnailUrl = result.ThumbnailUrl,
                mediaType = result.MediaType,
                imageWidth = result.ImageWidth,
                imageHeight = result.ImageHeight,
                videoDurationSeconds = result.VideoDurationSeconds
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return Problem(ex.Message, statusCode: 500);
        }
    }

    // ── Comments ─────────────────────────────────────────────────────────────────

    /// <summary>Returns all comments for a post (flat list — client builds the thread tree).</summary>
    [HttpGet("{postId:guid}/comments")]
    public async Task<IActionResult> GetComments(
        Guid postId, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetCommentsQuery(postId, UserId), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => Ok(result.Data),
            ResultCode.NotFound => NotFound(new { message = result.Error }),
            _ => Problem(result.Error, statusCode: 400)
        };
    }

    /// <summary>Adds a comment to a post. Caller must be an active member of the post's group.</summary>
    [HttpPost("{postId:guid}/comments")]
    public async Task<IActionResult> CreateComment(
        Guid postId, [FromBody] CreateCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new CreateCommentCommand(postId, UserId, request), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => Ok(result.Data),
            ResultCode.Forbidden => Forbid(),
            ResultCode.NotFound => NotFound(new { message = result.Error }),
            _ => Problem(result.Error, statusCode: 400)
        };
    }

    /// <summary>Updates a comment. Caller must be the comment author.</summary>
    [HttpPut("{postId:guid}/comments/{commentId:guid}")]
    public async Task<IActionResult> UpdateComment(
        Guid postId, Guid commentId, [FromBody] UpdateCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new UpdateCommentCommand(commentId, UserId, request), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => Ok(result.Data),
            ResultCode.Forbidden => Forbid(),
            ResultCode.NotFound => NotFound(new { message = result.Error }),
            _ => Problem(result.Error, statusCode: 400)
        };
    }

    /// <summary>Deletes a comment. Caller must be the author or a group admin.</summary>
    [HttpDelete("{postId:guid}/comments/{commentId:guid}")]
    public async Task<IActionResult> DeleteComment(
        Guid postId, Guid commentId,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new DeleteCommentCommand(commentId, UserId), cancellationToken);
        return result.Code switch
        {
            ResultCode.Ok => Ok(new { message = "Comment deleted." }),
            ResultCode.Forbidden => Forbid(),
            ResultCode.NotFound => NotFound(new { message = result.Error }),
            _ => Problem(result.Error, statusCode: 400)
        };
    }
}

using SoberNetwork.Core.DTOs;
using SoberNetwork.Core.DTOs.News;

namespace SoberNetwork.Core.Interfaces;

/// <summary>Platform-wide news and announcements feed operations.</summary>
public interface INewsService
{
    /// <summary>Gets approved posts (plus pending for admins) from the caller's groups, newest first.</summary>
    Task<(PagedResponse<PostResponse>? Posts, string? Error)> GetNewsFeedAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>Creates a post in the specified group. Auto-approves when RequiresPostApproval is false.</summary>
    Task<(PostResponse? Post, string? Error)> CreatePostAsync(
        Guid userId, CreatePostRequest request, CancellationToken ct = default);

    /// <summary>Updates a post. Caller must be the author.</summary>
    Task<(PostResponse? Post, string? Error)> UpdatePostAsync(
        Guid userId, Guid postId, UpdatePostRequest request, CancellationToken ct = default);

    /// <summary>Soft-deletes a post. Caller must be the author or an admin of the post's group.</summary>
    Task<(bool Success, string? Error)> DeletePostAsync(
        Guid userId, Guid postId, CancellationToken ct = default);

    /// <summary>Approves a pending post. Caller must be a GroupAdmin or SuperAdmin.</summary>
    Task<(bool Success, string? Error)> ApprovePostAsync(
        Guid userId, Guid postId, CancellationToken ct = default);

    // ── Comments ────────────────────────────────────────────────────────────────

    /// <summary>Returns all non-deleted comments for a post (flat; client builds the tree).</summary>
    Task<(IEnumerable<CommentResponse>? Comments, string? Error)> GetCommentsAsync(
        Guid postId, Guid requestingUserId, CancellationToken ct = default);

    /// <summary>Creates a comment on a post. Author must be an active member of the post's group.</summary>
    Task<(CommentResponse? Comment, string? Error)> CreateCommentAsync(
        Guid postId, Guid authorId, CreateCommentRequest request, CancellationToken ct = default);

    /// <summary>Updates a comment body. Caller must be the author.</summary>
    Task<(CommentResponse? Comment, string? Error)> UpdateCommentAsync(
        Guid commentId, Guid requestingUserId, string body, CancellationToken ct = default);

    /// <summary>Soft-deletes a comment. Caller must be the author or an admin of the post's group.</summary>
    Task<(bool Success, string? Error)> DeleteCommentAsync(
        Guid commentId, Guid requestingUserId, CancellationToken ct = default);
}

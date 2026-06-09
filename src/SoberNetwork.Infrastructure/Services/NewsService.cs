using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SoberNetwork.Core.DTOs;
using SoberNetwork.Core.DTOs.News;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Domain.Entities;
using SoberNetwork.Domain.Enums;
using SoberNetwork.Infrastructure.Data;

namespace SoberNetwork.Infrastructure.Services;

/// <summary>
/// Platform-wide news feed service. Posts are group-scoped but surfaced in a combined feed.
/// Members may create posts; admins may approve/delete; auto-approved when RequiresPostApproval is false.
/// </summary>
public sealed class NewsService(AppDbContext db) : INewsService
{
    /// <inheritdoc/>
    public async Task<(PagedResponse<PostResponse>? Posts, string? Error)> GetNewsFeedAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var memberGroupIds = await db.GroupMemberships
            .AsNoTracking()
            .Where(m => m.UserId == userId && m.Status == MemberStatus.Active && m.DeletedAt == null)
            .Select(m => m.GroupId)
            .ToListAsync(ct);

        if (memberGroupIds.Count == 0)
            return (new PagedResponse<PostResponse>([], page, pageSize, 0), null);

        var adminGroupIds = await db.GroupMemberships
            .AsNoTracking()
            .Where(m => m.UserId == userId && m.Status == MemberStatus.Active &&
                        m.Role == GroupRole.GroupAdmin && m.DeletedAt == null)
            .Select(m => m.GroupId)
            .ToHashSetAsync(ct);

        var isSuperAdmin = await db.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.IsSuperAdmin)
            .FirstOrDefaultAsync(ct);

        var query = db.Posts
            .AsNoTracking()
            .Include(p => p.Media)
            .Where(p =>
                memberGroupIds.Contains(p.GroupId) &&
                p.DeletedAt == null &&
                (p.IsApproved || adminGroupIds.Contains(p.GroupId) || isSuperAdmin))
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync(ct);

        // Project with comment count to avoid N+1
        var rawPosts = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                Post = p,
                GroupName = p.Group!.Name,
                GroupSlug = p.Group!.Slug,
                CommentCount = p.Comments.Count(c => c.DeletedAt == null)
            })
            .ToListAsync(ct);

        var authorIds = rawPosts.Select(r => r.Post.AuthorId).Distinct().ToList();
        var authors = await db.Users.AsNoTracking()
            .Where(u => authorIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.DisplayName, ct);

        var items = rawPosts.Select(r =>
            MapToResponse(r.Post, r.GroupName, r.GroupSlug,
                authors.GetValueOrDefault(r.Post.AuthorId, "Unknown"),
                adminGroupIds, isSuperAdmin, r.CommentCount)).ToList();

        return (new PagedResponse<PostResponse>(items, page, pageSize, totalCount), null);
    }

    /// <inheritdoc/>
    public async Task<(PostResponse? Post, string? Error)> CreatePostAsync(
        Guid userId, CreatePostRequest request, CancellationToken ct = default)
    {
        var group = await db.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Slug == request.GroupSlug && g.DeletedAt == null, ct);
        if (group == null) return (null, "Group not found.");

        var membership = await db.GroupMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(m =>
                m.GroupId == group.Id && m.UserId == userId &&
                m.Status == MemberStatus.Active && m.DeletedAt == null, ct);
        if (membership == null) return (null, "You are not a member of this group.");

        var author = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        var post = new Post
        {
            GroupId = group.Id,
            AuthorId = userId,
            Subject = request.Subject.Trim(),
            Body = request.Body.Trim(),
            MediaId = request.MediaId,  // Reference uploaded media
            LinkUrl = string.IsNullOrWhiteSpace(request.LinkUrl) ? null : request.LinkUrl.Trim(),
            LinkTitle = string.IsNullOrWhiteSpace(request.LinkTitle) ? null : request.LinkTitle.Trim(),
            IsApproved = !group.RequiresPostApproval,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        db.Posts.Add(post);
        await db.SaveChangesAsync(ct);

        // Link uploaded media to post if present
        if (request.MediaId.HasValue)
        {
            var media = await db.PostMedia
                .FirstOrDefaultAsync(m => m.Id == request.MediaId.Value && m.PostId == null, ct);
            if (media != null)
            {
                media.PostId = post.Id;
                await db.SaveChangesAsync(ct);
            }
        }

        post.Group = group;
        return (MapToResponse(post, group.Name, group.Slug, author?.DisplayName ?? "Unknown", [], isSuperAdmin: false, 0), null);
    }

    /// <inheritdoc/>
    public async Task<(PostResponse? Post, string? Error)> UpdatePostAsync(
        Guid userId, Guid postId, UpdatePostRequest request, CancellationToken ct = default)
    {
        var post = await db.Posts
            .Include(p => p.Group)
            .Include(p => p.Media)
            .FirstOrDefaultAsync(p => p.Id == postId && p.DeletedAt == null, ct);
        if (post == null) return (null, "Post not found.");

        if (post.AuthorId != userId)
            return (null, "Only the author may edit this post.");

        if (request.Subject != null) post.Subject = request.Subject.Trim();
        if (request.Body != null) post.Body = request.Body.Trim();
        post.LinkUrl = request.LinkUrl is null ? post.LinkUrl : (string.IsNullOrWhiteSpace(request.LinkUrl) ? null : request.LinkUrl.Trim());
        post.LinkTitle = request.LinkTitle is null ? post.LinkTitle : (string.IsNullOrWhiteSpace(request.LinkTitle) ? null : request.LinkTitle.Trim());
        
        // Handle media change
        if (request.MediaId.HasValue && request.MediaId.Value != post.MediaId)
        {
            post.MediaId = request.MediaId.Value;
            
            // Link the new media to this post
            var newMedia = await db.PostMedia
                .FirstOrDefaultAsync(m => m.Id == request.MediaId.Value && m.PostId == null, ct);
            if (newMedia != null)
            {
                newMedia.PostId = post.Id;
            }
        }
        
        post.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        var author = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
        return (MapToResponse(post, post.Group?.Name ?? "", post.Group?.Slug ?? "",
            author?.DisplayName ?? "Unknown", [], isSuperAdmin: false, 0), null);
    }

    /// <inheritdoc/>
    public async Task<(bool Success, string? Error)> DeletePostAsync(
        Guid userId, Guid postId, CancellationToken ct = default)
    {
        var post = await db.Posts
            .FirstOrDefaultAsync(p => p.Id == postId && p.DeletedAt == null, ct);
        if (post == null) return (false, "Post not found.");

        var isAdmin = await db.GroupMemberships.AnyAsync(m =>
            m.GroupId == post.GroupId && m.UserId == userId &&
            m.Role == GroupRole.GroupAdmin && m.Status == MemberStatus.Active && m.DeletedAt == null, ct);

        var isSuperAdmin = await db.Users.AnyAsync(u => u.Id == userId && u.IsSuperAdmin, ct);

        if (post.AuthorId != userId && !isAdmin && !isSuperAdmin)
            return (false, "You do not have permission to delete this post.");

        post.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return (true, null);
    }

    /// <inheritdoc/>
    public async Task<(bool Success, string? Error)> ApprovePostAsync(
        Guid userId, Guid postId, CancellationToken ct = default)
    {
        var post = await db.Posts
            .FirstOrDefaultAsync(p => p.Id == postId && p.DeletedAt == null, ct);
        if (post == null) return (false, "Post not found.");

        var isAdmin = await db.GroupMemberships.AnyAsync(m =>
            m.GroupId == post.GroupId && m.UserId == userId &&
            m.Role == GroupRole.GroupAdmin && m.Status == MemberStatus.Active && m.DeletedAt == null, ct);

        var isSuperAdmin = await db.Users.AnyAsync(u => u.Id == userId && u.IsSuperAdmin, ct);

        if (!isAdmin && !isSuperAdmin)
            return (false, "You do not have permission to approve this post.");

        post.IsApproved = true;
        post.ApprovedById = userId;
        post.ApprovedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return (true, null);
    }

    private static PostResponse MapToResponse(
        Post post, string groupName, string groupSlug, string authorDisplayName,
        ICollection<Guid> adminGroupIds, bool isSuperAdmin, int commentCount) =>
        new(
            post.Id,
            post.GroupId,
            groupName,
            groupSlug,
            post.AuthorId,
            authorDisplayName,
            post.Subject,
            post.Body,
        post.Media?.StoragePath != null
            ? $"/uploads/{post.Media.StoragePath.Replace("\\", "/")}"
            : null,  // MediaUrl
        post.Media?.ThumbnailPath != null
            ? $"/uploads/{post.Media.ThumbnailPath.Replace("\\", "/")}"
            : null,  // ThumbnailUrl
            post.Media?.MediaType,  // MediaType
            post.Media?.ImageWidth,
            post.Media?.ImageHeight,
            post.Media?.VideoDurationSeconds,
            post.LinkUrl,
            post.LinkTitle,
            post.IsApproved,
            NeedsApproval: !post.IsApproved && (adminGroupIds.Contains(post.GroupId) || isSuperAdmin),
            post.CreatedAt,
            post.UpdatedAt,
            CommentCount: commentCount
        );

    // ── Comment methods ─────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<(IEnumerable<CommentResponse>? Comments, string? Error)> GetCommentsAsync(
        Guid postId, Guid requestingUserId, CancellationToken ct = default)
    {
        var post = await db.Posts.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == postId && p.DeletedAt == null, ct);
        if (post == null) return (null, "Post not found.");

        var comments = await db.PostComments.AsNoTracking()
            .Include(c => c.Media)
            .Where(c => c.PostId == postId && c.DeletedAt == null)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(ct);

        var authorIds = comments.Select(c => c.AuthorId).Distinct().ToList();
        var authors = await db.Users.AsNoTracking()
            .Where(u => authorIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.DisplayName, ct);

        return (comments.Select(c => MapCommentToResponse(c, authors)), null);
    }

    /// <inheritdoc/>
    public async Task<(CommentResponse? Comment, string? Error)> CreateCommentAsync(
        Guid postId, Guid authorId, CreateCommentRequest request, CancellationToken ct = default)
    {
        var post = await db.Posts.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == postId && p.DeletedAt == null, ct);
        if (post == null) return (null, "Post not found.");

        var membership = await db.GroupMemberships.AsNoTracking()
            .FirstOrDefaultAsync(m =>
                m.GroupId == post.GroupId && m.UserId == authorId &&
                m.Status == MemberStatus.Active && m.DeletedAt == null, ct);
        if (membership == null) return (null, "You are not a member of this group.");

        // Validate parent comment exists and belongs to the same post
        if (request.ParentCommentId.HasValue)
        {
            var parentExists = await db.PostComments.AsNoTracking()
                .AnyAsync(c => c.Id == request.ParentCommentId.Value &&
                               c.PostId == postId && c.DeletedAt == null, ct);
            if (!parentExists) return (null, "Parent comment not found.");
        }

        var comment = new PostComment
        {
            PostId = postId,
            AuthorId = authorId,
            ParentCommentId = request.ParentCommentId,
            MediaId = request.MediaId,
            Body = request.Body.Trim(),
            LinkUrl = string.IsNullOrWhiteSpace(request.LinkUrl) ? null : request.LinkUrl.Trim(),
            LinkTitle = string.IsNullOrWhiteSpace(request.LinkTitle) ? null : request.LinkTitle.Trim(),
        };

        db.PostComments.Add(comment);
        await db.SaveChangesAsync(ct);

        // Link uploaded media to comment if present
        PostMedia? media = null;
        if (request.MediaId.HasValue)
        {
            media = await db.PostMedia
                .FirstOrDefaultAsync(m => m.Id == request.MediaId.Value && m.PostId == null && m.CommentId == null, ct);
            if (media != null)
            {
                media.CommentId = comment.Id;
                await db.SaveChangesAsync(ct);
                comment.Media = media;
            }
        }

        var author = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == authorId, ct);
        return (MapCommentToResponse(comment, new Dictionary<Guid, string>
        {
            [authorId] = author?.DisplayName ?? "Unknown"
        }), null);
    }

    /// <inheritdoc/>
    public async Task<(CommentResponse? Comment, string? Error)> UpdateCommentAsync(
        Guid commentId, Guid requestingUserId, UpdateCommentRequest request, CancellationToken ct = default)
    {
        var comment = await db.PostComments
            .Include(c => c.Media)
            .FirstOrDefaultAsync(c => c.Id == commentId && c.DeletedAt == null, ct);
        if (comment == null) return (null, "Comment not found.");

        if (comment.AuthorId != requestingUserId)
            return (null, "Only the author may edit this comment.");

        comment.Body = request.Body.Trim();
        comment.LinkUrl = request.LinkUrl is null ? comment.LinkUrl : (string.IsNullOrWhiteSpace(request.LinkUrl) ? null : request.LinkUrl.Trim());
        comment.LinkTitle = request.LinkTitle is null ? comment.LinkTitle : (string.IsNullOrWhiteSpace(request.LinkTitle) ? null : request.LinkTitle.Trim());

        // Handle media change
        if (request.MediaId.HasValue && request.MediaId.Value != comment.MediaId)
        {
            comment.MediaId = request.MediaId.Value;
            var newMedia = await db.PostMedia
                .FirstOrDefaultAsync(m => m.Id == request.MediaId.Value && m.PostId == null && m.CommentId == null, ct);
            if (newMedia != null)
            {
                newMedia.CommentId = comment.Id;
                comment.Media = newMedia;
            }
        }

        comment.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        var author = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == requestingUserId, ct);
        return (MapCommentToResponse(comment, new Dictionary<Guid, string>
        {
            [requestingUserId] = author?.DisplayName ?? "Unknown"
        }), null);
    }

    /// <inheritdoc/>
    public async Task<(bool Success, string? Error)> DeleteCommentAsync(
        Guid commentId, Guid requestingUserId, CancellationToken ct = default)
    {
        var comment = await db.PostComments
            .Include(c => c.Post)
            .FirstOrDefaultAsync(c => c.Id == commentId && c.DeletedAt == null, ct);
        if (comment == null) return (false, "Comment not found.");

        var isAdmin = await db.GroupMemberships.AnyAsync(m =>
            m.GroupId == comment.Post.GroupId && m.UserId == requestingUserId &&
            m.Role == GroupRole.GroupAdmin && m.Status == MemberStatus.Active && m.DeletedAt == null, ct);

        var isSuperAdmin = await db.Users.AnyAsync(u => u.Id == requestingUserId && u.IsSuperAdmin, ct);

        if (comment.AuthorId != requestingUserId && !isAdmin && !isSuperAdmin)
            return (false, "You do not have permission to delete this comment.");

        comment.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return (true, null);
    }

    private static CommentResponse MapCommentToResponse(PostComment c, Dictionary<Guid, string> authors) =>
        new(c.Id, c.PostId, c.ParentCommentId,
            c.AuthorId, authors.GetValueOrDefault(c.AuthorId, "Unknown"),
            c.Body,
            c.Media?.StoragePath != null ? $"/uploads/{c.Media.StoragePath.Replace("\\", "/")}" : null,
            c.Media?.MediaType,
            c.Media?.Id,
            c.LinkUrl, c.LinkTitle, c.CreatedAt, c.UpdatedAt);
}

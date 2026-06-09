using MediatR;
using Microsoft.EntityFrameworkCore;
using SoberNetwork.Core.Commands.News;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Infrastructure.Data;

namespace SoberNetwork.Infrastructure.Handlers.News;

/// <summary>Handler for uploading post media (images/videos).</summary>
public sealed class UploadPostMediaCommandHandler(AppDbContext db, IMediaService mediaService)
    : IRequestHandler<UploadPostMediaCommand, UploadPostMediaResult>
{
    public async Task<UploadPostMediaResult> Handle(UploadPostMediaCommand request, CancellationToken ct)
    {
        // Verify group exists
        var group = await db.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == request.GroupId && g.DeletedAt == null, ct);

        if (group == null)
            throw new InvalidOperationException("Group not found.");

        // Upload to storage service
        var result = await mediaService.UploadMediaAsync(
            request.GroupId,
            Guid.Empty,  // PostId unknown at upload time
            request.FileStream,
            request.FileName,
            request.ContentType,
            ct);

        if (!result.Success)
            throw new InvalidOperationException(result.Error ?? "Upload failed.");

        var uploadResult = result.Data!;
        return new UploadPostMediaResult(
            uploadResult.MediaId,
            uploadResult.MediaUrl,
            uploadResult.ThumbnailUrl,
            uploadResult.MediaType,
            uploadResult.ImageWidth,
            uploadResult.ImageHeight,
            uploadResult.VideoDurationSeconds);
    }
}


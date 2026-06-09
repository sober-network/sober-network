using MediatR;

namespace SoberNetwork.Core.Commands.News;

/// <summary>Command to upload an image or video for a post.</summary>
public record UploadPostMediaCommand(
    Guid GroupId,
    Stream FileStream,
    string FileName,
    string ContentType
) : IRequest<UploadPostMediaResult>;

/// <summary>Result of a successful media upload.</summary>
public record UploadPostMediaResult(
    Guid MediaId,
    string MediaUrl,
    string? ThumbnailUrl,
    string MediaType,
    int? ImageWidth,
    int? ImageHeight,
    int? VideoDurationSeconds
);

namespace SoberNetwork.Core.DTOs.News;

public record CreateCommentRequest(
    string Body,
    Guid? ParentCommentId = null
);

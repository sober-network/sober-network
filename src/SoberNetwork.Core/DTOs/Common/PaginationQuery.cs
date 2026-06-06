namespace SoberNetwork.Core.DTOs.Common;

/// <summary>Standard pagination parameters bound from the query string. Validated at the boundary.</summary>
public record PaginationQuery
{
    /// <summary>1-based page number. Defaults to 1.</summary>
    public int Page { get; init; } = 1;

    /// <summary>Page size (1–100). Defaults to 25.</summary>
    public int PageSize { get; init; } = 25;
}

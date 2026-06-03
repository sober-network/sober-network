namespace SoberNetwork.Core.DTOs;

/// <summary>Generic pagination wrapper used by list endpoints.</summary>
public record PagedResponse<T>(
    /// <summary>Items returned for the current page.</summary>
    IReadOnlyList<T> Items,
    /// <summary>1-based page number requested by the client.</summary>
    int Page,
    /// <summary>Number of items requested per page.</summary>
    int PageSize,
    /// <summary>Total number of matching items across all pages.</summary>
    int TotalCount)
{
    /// <summary>Total number of pages available for the current result set.</summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

    /// <summary>Whether another page exists after the current page.</summary>
    public bool HasNext => Page < TotalPages;

    /// <summary>Whether a page exists before the current page.</summary>
    public bool HasPrev => Page > 1;
}

namespace SoberNetwork.Core.Results;

/// <summary>Result for commands and queries that return data.</summary>
public record DataResult<T>(ResultCode Code, T? Data, string? Error = null)
{
    /// <summary>Whether the operation completed successfully.</summary>
    public bool Success => Code == ResultCode.Ok;

    /// <summary>Creates a successful result carrying data.</summary>
    public static DataResult<T> Ok(T data) => new(ResultCode.Ok, data);

    /// <summary>Creates a failed result with the given code and error message.</summary>
    public static DataResult<T> Fail(ResultCode code, string error) => new(code, default, error);
}

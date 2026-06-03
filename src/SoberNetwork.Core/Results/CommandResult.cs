namespace SoberNetwork.Core.Results;

/// <summary>Result for commands that do not return data.</summary>
public record CommandResult(ResultCode Code, string? Error = null)
{
    /// <summary>Whether the command completed successfully.</summary>
    public bool Success => Code == ResultCode.Ok;

    /// <summary>Creates a successful result.</summary>
    public static CommandResult Ok() => new(ResultCode.Ok);

    /// <summary>Creates a failed result with the given code and error message.</summary>
    public static CommandResult Fail(ResultCode code, string error) => new(code, error);
}

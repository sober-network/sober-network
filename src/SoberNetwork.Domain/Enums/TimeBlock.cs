namespace SoberNetwork.Domain.Enums;

/// <summary>Time-of-day bucket used for meeting schedule filtering.</summary>
public enum TimeBlock
{
    /// <summary>06:00–11:59</summary>
    Morning = 0,

    /// <summary>12:00–16:59</summary>
    Afternoon = 1,

    /// <summary>17:00–20:59</summary>
    Evening = 2,

    /// <summary>21:00–05:59 (overnight)</summary>
    Night = 3
}

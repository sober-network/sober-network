using SoberNetwork.Core.DTOs;

namespace SoberNetwork.Core.Interfaces;

/// <summary>Provides aggregate platform statistics for public display.</summary>
public interface IStatsService
{
    /// <summary>Returns the current counts of active members, groups, and meetings.</summary>
    Task<PlatformStatsResponse> GetPlatformStatsAsync(CancellationToken cancellationToken = default);
}

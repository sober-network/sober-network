using MediatR;
using SoberNetwork.Core.DTOs;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Core.Queries;

namespace SoberNetwork.Core.Handlers;

/// <summary>Returns aggregate platform statistics. No authentication required.</summary>
public class GetPlatformStatsQueryHandler(IStatsService statsService)
    : IRequestHandler<GetPlatformStatsQuery, PlatformStatsResponse>
{
    public Task<PlatformStatsResponse> Handle(
        GetPlatformStatsQuery request, CancellationToken cancellationToken) =>
        statsService.GetPlatformStatsAsync(cancellationToken);
}

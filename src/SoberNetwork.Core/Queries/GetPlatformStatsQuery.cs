using MediatR;
using SoberNetwork.Core.DTOs;

namespace SoberNetwork.Core.Queries;

/// <summary>Returns aggregate platform statistics for the public landing page.</summary>
public record GetPlatformStatsQuery : IRequest<PlatformStatsResponse>;

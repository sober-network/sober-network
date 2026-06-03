using MediatR;
using Microsoft.Extensions.Logging;

namespace SoberNetwork.Core.Behaviors;

/// <summary>
/// Logs the start and completion of every MediatR request.
/// IMPORTANT: Never logs request or response contents — commands may carry sensitive auth data.
/// </summary>
public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseRequest
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        logger.LogDebug("Handling {RequestName}", requestName);
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var response = await next();
            sw.Stop();
            logger.LogDebug("Handled {RequestName} in {ElapsedMs}ms", requestName, sw.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex, "Error handling {RequestName} after {ElapsedMs}ms", requestName, sw.ElapsedMilliseconds);
            throw;
        }
    }
}

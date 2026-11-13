using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace CulinaryBlog.API.HealthChecks;

public sealed class RedisHealthCheck(IConnectionMultiplexer connection) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!connection.IsConnected)
                return HealthCheckResult.Unhealthy("Redis is unavailable.");
            await connection.GetDatabase().PingAsync().WaitAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Unhealthy("Redis is unavailable.");
        }
    }
}

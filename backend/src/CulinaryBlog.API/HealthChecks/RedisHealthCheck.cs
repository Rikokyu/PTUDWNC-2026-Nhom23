using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace CulinaryBlog.API.HealthChecks;

public sealed class RedisHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public RedisHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var connectionString = _configuration.GetConnectionString("Redis");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return HealthCheckResult.Unhealthy(
                "Redis connection string is not configured.");
        }

        try
        {
            var options = ConfigurationOptions.Parse(connectionString);
            options.ConnectTimeout = 2000;
            options.AsyncTimeout = 2000;
            options.AbortOnConnectFail = true;

            using var connection = await ConnectionMultiplexer.ConnectAsync(options);
            await connection.GetDatabase().PingAsync();
            return HealthCheckResult.Healthy();
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Unhealthy("Redis health check timed out.");
        }
        catch (Exception)
        {
            return HealthCheckResult.Unhealthy("Redis is unavailable.");
        }
    }
}

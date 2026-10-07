using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CulinaryBlog.API.HealthChecks;

public sealed class PostgreSqlHealthCheck : IHealthCheck
{
    private readonly CulinaryBlogDbContext _dbContext;

    public PostgreSqlHealthCheck(CulinaryBlogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _dbContext.Database
                .CanConnectAsync(cancellationToken);

            return canConnect
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("PostgreSQL is unavailable.");
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Unhealthy(
                "PostgreSQL health check timed out.");
        }
        catch (Exception)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL is unavailable.");
        }
    }
}
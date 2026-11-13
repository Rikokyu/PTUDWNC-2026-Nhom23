using Microsoft.Extensions.Diagnostics.HealthChecks;
using Minio;
using Minio.DataModel.Args;

namespace CulinaryBlog.API.HealthChecks;

public sealed class MinioHealthCheck(
    IConfiguration configuration,
    IMinioClient client) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var bucket = configuration["Minio:Bucket"];
        if (string.IsNullOrWhiteSpace(bucket))
            return HealthCheckResult.Unhealthy("MinIO bucket is not configured.");

        try
        {
            var exists = await client.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(bucket), cancellationToken);
            return exists
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("MinIO bucket is unavailable.");
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Unhealthy("MinIO is unavailable.");
        }
    }
}

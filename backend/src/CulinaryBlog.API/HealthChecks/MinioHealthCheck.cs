using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CulinaryBlog.API.HealthChecks;

public sealed class MinioHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public MinioHealthCheck(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var endpoint = _configuration["Minio:Endpoint"];
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return HealthCheckResult.Unhealthy(
                "MinIO endpoint is not configured.");
        }

        var useSsl = bool.TryParse(_configuration["Minio:UseSSL"], out var ssl)
            && ssl;
        var scheme = useSsl ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;
        var endpointUrl = endpoint.Contains("://", StringComparison.Ordinal)
            ? endpoint
            : $"{scheme}://{endpoint}";

        if (!Uri.TryCreate(endpointUrl, UriKind.Absolute, out var serviceUri))
        {
            return HealthCheckResult.Unhealthy(
                "MinIO endpoint is invalid.");
        }

        var healthUri = new Uri(
            serviceUri,
            "/minio/health/ready");

        try
        {
            using var response = await _httpClientFactory
                .CreateClient("MinioHealthCheck")
                .GetAsync(healthUri, cancellationToken);

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("MinIO is unavailable.");
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Unhealthy("MinIO health check timed out.");
        }
        catch (Exception)
        {
            return HealthCheckResult.Unhealthy("MinIO is unavailable.");
        }
    }
}

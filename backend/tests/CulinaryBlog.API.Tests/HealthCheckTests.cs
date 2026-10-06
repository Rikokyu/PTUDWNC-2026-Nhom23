using System.Net;
using CulinaryBlog.API.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace CulinaryBlog.API.Tests;

public sealed class HealthCheckTests
{
    [Fact]
    public async Task RedisHealthCheck_ReturnsUnhealthy_WhenConnectionStringIsMissing()
    {
        var configuration = CreateConfiguration();
        var healthCheck = new RedisHealthCheck(configuration);

        var result = await healthCheck.CheckHealthAsync(
            new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }

    [Fact]
    public async Task MinioHealthCheck_ReturnsUnhealthy_WhenEndpointIsMissing()
    {
        var healthCheck = new MinioHealthCheck(
            CreateConfiguration(),
            new TestHttpClientFactory(new HttpClient()));

        var result = await healthCheck.CheckHealthAsync(
            new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }

    [Fact]
    public async Task MinioHealthCheck_RequestsReadinessEndpoint()
    {
        var handler = new StubHttpMessageHandler(
            _ => new HttpResponseMessage(HttpStatusCode.OK));
        var healthCheck = new MinioHealthCheck(
            CreateConfiguration(new Dictionary<string, string?>
            {
                ["Minio:Endpoint"] = "minio.example:9000"
            }),
            new TestHttpClientFactory(new HttpClient(handler)));

        var result = await healthCheck.CheckHealthAsync(
            new HealthCheckContext());

        Assert.Equal(HealthStatus.Healthy, result.Status);
        Assert.Equal(
            "/minio/health/ready",
            handler.RequestUri?.AbsolutePath);
    }

    [Fact]
    public async Task MinioHealthCheck_ReturnsUnhealthy_WhenServiceIsUnavailable()
    {
        var handler = new StubHttpMessageHandler(
            _ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        var healthCheck = new MinioHealthCheck(
            CreateConfiguration(new Dictionary<string, string?>
            {
                ["Minio:Endpoint"] = "minio.example:9000"
            }),
            new TestHttpClientFactory(new HttpClient(handler)));

        var result = await healthCheck.CheckHealthAsync(
            new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }

    private static IConfiguration CreateConfiguration(
        IDictionary<string, string?>? values = null) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(values ?? new Dictionary<string, string?>())
            .Build();

    private sealed class TestHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;

        public TestHttpClientFactory(HttpClient client)
        {
            _client = client;
        }

        public HttpClient CreateClient(string name) => _client;
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory;

        public StubHttpMessageHandler(
            Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        {
            _responseFactory = responseFactory;
        }

        public Uri? RequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            return Task.FromResult(_responseFactory(request));
        }
    }
}

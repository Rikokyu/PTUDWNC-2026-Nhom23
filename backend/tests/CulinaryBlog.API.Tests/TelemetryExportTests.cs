using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;
using CulinaryBlog.API.Extensions;
using CulinaryBlog.Application.Common.Behaviors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Xunit;

namespace CulinaryBlog.API.Tests;

public sealed class TelemetryExportTests
{
    [Fact]
    public async Task OtlpHttpExporterSendsTraceAndMetricProtobufToReceiver()
    {
        var received = new ConcurrentDictionary<string, byte[]>();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        await using var app = builder.Build();
        app.MapPost("/v1/{signal}", async (string signal, HttpRequest request) =>
        {
            using var body = new MemoryStream();
            await request.Body.CopyToAsync(body);
            Assert.Equal("application/x-protobuf", request.ContentType);
            received[signal] = body.ToArray();
            return Results.Bytes([], "application/x-protobuf");
        });
        await app.StartAsync();
        var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        using var traces = Sdk.CreateTracerProviderBuilder().AddSource("export.test")
            .AddOtlpExporter(options => { options.Endpoint = new Uri(address + "/v1/traces"); options.Protocol = OtlpExportProtocol.HttpProtobuf; }).Build();
        using var metrics = Sdk.CreateMeterProviderBuilder().AddMeter("export.test")
            .AddOtlpExporter(options => { options.Endpoint = new Uri(address + "/v1/metrics"); options.Protocol = OtlpExportProtocol.HttpProtobuf; }).Build();
        using var source = new ActivitySource("export.test");
        using (var span = source.StartActivity("export-verification")) Assert.NotNull(span);
        using var meter = new Meter("export.test");
        meter.CreateCounter<long>("export.verification").Add(1);
        Assert.True(traces.ForceFlush(5000));
        Assert.True(metrics.ForceFlush(5000));
        Assert.NotEmpty(received["traces"]);
        Assert.NotEmpty(received["metrics"]);
        await app.StopAsync();
    }

    [Fact]
    public async Task RequestPropagatesW3cTraceAndSurvivesUnavailableCollector()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["OpenTelemetry:Enabled"] = "true", ["OpenTelemetry:ExportEnabled"] = "true",
            ["OpenTelemetry:Endpoint"] = "http://127.0.0.1:1"
        });
        builder.Services.AddCulinaryTelemetry(builder.Configuration);
        await using var app = builder.Build();
        app.MapGet("/probe", () =>
        {
            using var child = CulinaryTelemetry.Source.StartActivity("probe");
            return Results.Ok(new { traceId = Activity.Current!.TraceId.ToString(), spanId = Activity.Current.SpanId.ToString() });
        });
        await app.StartAsync();
        var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        using var client = new HttpClient(new SocketsHttpHandler { ActivityHeadersPropagator = null });
        using var request = new HttpRequestMessage(HttpMethod.Get, address + "/probe");
        request.Headers.Add("traceparent", "00-0123456789abcdef0123456789abcdef-0123456789abcdef-01");
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("0123456789abcdef0123456789abcdef", await response.Content.ReadAsStringAsync());
        await app.StopAsync();
    }
}

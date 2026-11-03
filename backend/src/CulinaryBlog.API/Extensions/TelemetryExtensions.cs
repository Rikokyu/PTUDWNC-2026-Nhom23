using CulinaryBlog.Application.Common.Behaviors;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace CulinaryBlog.API.Extensions;

public static class TelemetryExtensions
{
    public static IServiceCollection AddCulinaryTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        if (!configuration.GetValue("OpenTelemetry:Enabled", true)) return services;
        var export = configuration.GetValue("OpenTelemetry:ExportEnabled", false);
        void ConfigureExporter(OpenTelemetry.Exporter.OtlpExporterOptions options)
        {
            var endpoint = configuration["OpenTelemetry:Endpoint"];
            if (!string.IsNullOrWhiteSpace(endpoint)) options.Endpoint = new Uri(endpoint);
            options.TimeoutMilliseconds = 2000;
        }
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("CulinaryBlog.API"))
            .WithTracing(tracing =>
            {
                tracing.AddSource(CulinaryTelemetry.Name, "Npgsql")
                    .AddAspNetCoreInstrumentation().AddHttpClientInstrumentation();
                if (export) tracing.AddOtlpExporter(ConfigureExporter);
            })
            .WithMetrics(metrics =>
            {
                metrics.AddMeter(CulinaryTelemetry.Name, "Microsoft.AspNetCore.Hosting", "Microsoft.AspNetCore.Diagnostics", "Npgsql")
                    .AddAspNetCoreInstrumentation().AddHttpClientInstrumentation();
                if (export) metrics.AddOtlpExporter(ConfigureExporter);
            });
        return services;
    }
}

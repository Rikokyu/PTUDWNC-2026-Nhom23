using CulinaryBlog.API.Extensions;
using CulinaryBlog.API.HealthChecks;
using CulinaryBlog.API.Middleware;
using CulinaryBlog.Infrastructure.Persistence.Seed;
using CulinaryBlog.Application;
using CulinaryBlog.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json.Serialization;
using System.Security.Claims;
using Microsoft.OpenApi;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http.Features;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

var builder =
    WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    var minimumLevel = context.HostingEnvironment.IsDevelopment()
        ? LogEventLevel.Debug
        : LogEventLevel.Information;

    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .MinimumLevel.Is(minimumLevel)
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
        .Enrich.WithProperty("ApplicationName", context.HostingEnvironment.ApplicationName)
        .WriteTo.Console(new JsonFormatter())
        .WriteTo.File(
            new JsonFormatter(),
            "logs/culinaryblog-.json",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 14,
            shared: true);

    var seqServerUrl = context.Configuration["Seq:ServerUrl"];
    if (Uri.TryCreate(seqServerUrl, UriKind.Absolute, out _))
    {
        loggerConfiguration.WriteTo.Seq(seqServerUrl!);
    }
});

// Application
builder.Services
    .AddApplicationServices();

// Infrastructure
builder.Services
    .AddInfrastructure(
        builder.Configuration);

builder.Services
    .AddHealthChecks()
    .AddCheck(
        "self",
        () => HealthCheckResult.Healthy(),
        tags: new[] { "live" })
    .AddCheck<PostgreSqlHealthCheck>(
        "postgresql",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "ready" },
        timeout: TimeSpan.FromSeconds(3))
    .AddCheck<RedisHealthCheck>(
        "redis",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "ready" },
        timeout: TimeSpan.FromSeconds(3))
    .AddCheck<MinioHealthCheck>(
        "minio",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "ready" },
        timeout: TimeSpan.FromSeconds(3));

builder.Services.AddHttpClient("MinioHealthCheck", client =>
{
    client.Timeout = TimeSpan.FromSeconds(3);
});

// Output Cache
builder.Services
    .AddOutputCache(options =>
    {
        options.AddPolicy(
            "RecipeList",
            policy =>
            {
                policy
                    .Expire(
                        TimeSpan.FromMinutes(15))
                    .Tag("recipes")
                    .SetVaryByQuery("*");
            });

        options.AddPolicy(
            "RecipeDetail",
            policy =>
            {
                policy
                    .Expire(
                        TimeSpan.FromMinutes(60))
                    .Tag("recipes");
            });
    });

// Swagger
builder.Services
    .AddEndpointsApiExplorer();

builder.Services
    .AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT access token"
        });
        options.OperationFilter<AuthorizeOperationFilter>();
    });

builder.Services
    .AddProblemDetails();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("AuthLogin", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var maxFileSizeBytes = builder.Configuration.GetValue<long>(
    "FileUpload:MaxFileSizeBytes",
    5 * 1024 * 1024);
if (maxFileSizeBytes <= 0)
{
    throw new InvalidOperationException("FileUpload:MaxFileSizeBytes must be positive.");
}

builder.Services.Configure<FormOptions>(options =>
    options.MultipartBodyLengthLimit = maxFileSizeBytes + 64 * 1024);

var app =
    builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.GetLevel = (httpContext, elapsedMilliseconds, exception) =>
    {
        httpContext.Items["ElapsedMilliseconds"] = elapsedMilliseconds;
        return exception is not null || httpContext.Response.StatusCode >= 500
            ? LogEventLevel.Error
            : httpContext.Response.StatusCode >= 400
                ? LogEventLevel.Warning
                : elapsedMilliseconds > 500
                    ? LogEventLevel.Warning
                    : LogEventLevel.Information;
    };
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set(
            "CorrelationId",
            CorrelationIdMiddleware.GetCorrelationId(httpContext));
        diagnosticContext.Set("RequestMethod", httpContext.Request.Method);
        diagnosticContext.Set("RequestPath", httpContext.Request.Path.Value);
        diagnosticContext.Set("StatusCode", httpContext.Response.StatusCode);
        diagnosticContext.Set(
            "ElapsedMilliseconds",
            httpContext.Items["ElapsedMilliseconds"]);

        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(userId))
        {
            diagnosticContext.Set("UserId", userId);
        }
    };
});

// Global exception middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseStatusCodePages();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Output cache
app.UseOutputCache();

app.UseHttpsRedirection();

// API endpoints
app.MapApplicationEndpoints();

// Seed database
using (var scope =
       app.Services.CreateScope())
{
    var context =
        scope.ServiceProvider
            .GetRequiredService<
                CulinaryBlog.Infrastructure
                    .Persistence
                    .CulinaryBlogDbContext>();

    try
    {
        await DatabaseSeeder
            .SeedAsync(context);
    }
    catch (DbUpdateConcurrencyException ex)
    {
        app.Logger.LogWarning(
            ex,
            "Database seed detected a concurrency conflict; app will continue startup with existing database state.");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(
            ex,
            "Database seed failed during startup.");
    }
}

app.Run();
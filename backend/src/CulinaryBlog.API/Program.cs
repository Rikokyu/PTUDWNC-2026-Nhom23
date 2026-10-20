using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Security.Claims;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using CulinaryBlog.API.Extensions;
using CulinaryBlog.API.Authorization;
using CulinaryBlog.API.HealthChecks;
using CulinaryBlog.API.Middleware;
using CulinaryBlog.Infrastructure.Persistence.Seed;
using CulinaryBlog.Application;
using CulinaryBlog.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.OpenApi;
using Hangfire;
using Asp.Versioning;

var builder =
    WebApplication.CreateBuilder(args);

var webRootPath = Path.Combine(
    builder.Environment.ContentRootPath,
    "wwwroot");
Directory.CreateDirectory(Path.Combine(webRootPath, "uploads"));
builder.Environment.WebRootPath = webRootPath;

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

builder.Services
    .AddJwtAuthentication(builder.Configuration);

builder.Services
    .AddFrontendCors(builder.Configuration);
builder.Services
    .AddApiRateLimiting();

builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new HeaderApiVersionReader("api-version");
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
    });

// Infrastructure
builder.Services
    .AddInfrastructure(
        builder.Configuration,
        builder.Environment.ContentRootPath);

builder.Services.AddAuthorization();

// Output Cache
builder.Services
    .AddOutputCache(options =>
    {
        options.AddPolicy(
            "RecipeList",
            new RecipeListOutputCachePolicy());

        options.AddPolicy(
            "RecipeDetail",
            new RecipeDetailOutputCachePolicy());
    });

// Swagger
builder.Services
    .AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "CulinaryBlog API",
            Version = "v1",
            Description = "Version 1 of the CulinaryBlog HTTP API."
        });

    var xmlDocumentationFile = Path.Combine(
        AppContext.BaseDirectory,
        $"{typeof(Program).Assembly.GetName().Name}.xml");
    if (File.Exists(xmlDocumentationFile))
    {
        options.IncludeXmlComments(xmlDocumentationFile);
    }

    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Paste a valid JWT access token."
        });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(
                "Bearer",
                document,
                null)] = []
        });
});

builder.Services
    .AddProblemDetails();

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

        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? httpContext.User.FindFirstValue("sub");
        if (!string.IsNullOrWhiteSpace(userId))
        {
            diagnosticContext.Set("UserId", userId);
        }
    };
});

// Global exception middleware
app.UseMiddleware<
    GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "CulinaryBlog API v1"));
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("Frontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();

// Output cache
app.UseOutputCache();

if (builder.Configuration.GetValue("Hangfire:Enabled", true)
    && (!app.Environment.IsDevelopment()
        || builder.Configuration.GetValue<bool>("Hangfire:DashboardEnabled")))
{
    app.UseHangfireDashboard(
        "/hangfire",
        new DashboardOptions
        {
            Authorization = [new HangfireAdminAuthorizationFilter()]
        });
}

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
            .SeedAsync(
                context,
                app.Lifetime.ApplicationStopping);
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

internal sealed class RecipeListOutputCachePolicy : IOutputCachePolicy
{
    public ValueTask CacheRequestAsync(
        OutputCacheContext context,
        CancellationToken cancellationToken)
    {
        var isPublicRequest =
            context.HttpContext.User.Identity?.IsAuthenticated != true;

        context.EnableOutputCaching = true;
        context.AllowCacheLookup = isPublicRequest;
        context.AllowCacheStorage = isPublicRequest;
        context.ResponseExpirationTimeSpan =
            TimeSpan.FromMinutes(15);
        context.CacheVaryByRules.QueryKeys =
            new Microsoft.Extensions.Primitives.StringValues("*");

        return ValueTask.CompletedTask;
    }

    public ValueTask ServeFromCacheAsync(
        OutputCacheContext context,
        CancellationToken cancellationToken) =>
        ValueTask.CompletedTask;

    public ValueTask ServeResponseAsync(
        OutputCacheContext context,
        CancellationToken cancellationToken)
    {
        context.Tags.Add("recipes");
        return ValueTask.CompletedTask;
    }
}

internal sealed class RecipeDetailOutputCachePolicy : IOutputCachePolicy
{
    public ValueTask CacheRequestAsync(
        OutputCacheContext context,
        CancellationToken cancellationToken)
    {
        var isPublicRequest =
            context.HttpContext.User.Identity?.IsAuthenticated != true;

        context.EnableOutputCaching = true;
        context.AllowCacheLookup = isPublicRequest;
        context.AllowCacheStorage = isPublicRequest;
        context.ResponseExpirationTimeSpan =
            TimeSpan.FromMinutes(60);

        return ValueTask.CompletedTask;
    }

    public ValueTask ServeFromCacheAsync(
        OutputCacheContext context,
        CancellationToken cancellationToken) =>
        ValueTask.CompletedTask;

    public ValueTask ServeResponseAsync(
        OutputCacheContext context,
        CancellationToken cancellationToken)
    {
        context.Tags.Add("recipes");

        if (context.HttpContext.Request.RouteValues.TryGetValue(
                "slug",
                out var slug)
            && slug is string slugValue
            && !string.IsNullOrWhiteSpace(slugValue))
        {
            context.Tags.Add($"recipe:{slugValue}");
        }

        return ValueTask.CompletedTask;
    }
}

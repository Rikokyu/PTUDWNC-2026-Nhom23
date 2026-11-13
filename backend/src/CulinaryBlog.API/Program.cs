using CulinaryBlog.API.Extensions;
using CulinaryBlog.API.Authorization;
using CulinaryBlog.API.HealthChecks;
using CulinaryBlog.API.Middleware;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.Infrastructure.Persistence.Seed;
using CulinaryBlog.Application;
using CulinaryBlog.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.OpenApi;
using Hangfire;
using Asp.Versioning;
<<<<<<< HEAD
=======
using CulinaryBlog.Application.Common.Interfaces;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using CulinaryBlog.API.Observability;
using CulinaryBlog.API.Caching;
using StackExchange.Redis;

>>>>>>> 315339d (Update UI)

var builder =
    WebApplication.CreateBuilder(args);

<<<<<<< HEAD
=======
builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .WriteTo.Console(new CompactJsonFormatter())
        .WriteTo.File(
            new CompactJsonFormatter(),
            Path.Combine(context.HostingEnvironment.ContentRootPath, "logs", "api-.json"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 14,
            shared: true);
    if (context.HostingEnvironment.IsDevelopment()
        && Uri.TryCreate(context.Configuration["Seq:ServerUrl"],
            UriKind.Absolute, out var seqUri)
        && seqUri.Scheme is "http" or "https")
        loggerConfiguration.WriteTo.Seq(seqUri.ToString());
});

>>>>>>> 315339d (Update UI)
var webRootPath = Path.Combine(
    builder.Environment.ContentRootPath,
    "wwwroot");
Directory.CreateDirectory(Path.Combine(webRootPath, "uploads"));
builder.Environment.WebRootPath = webRootPath;

<<<<<<< HEAD
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

=======
>>>>>>> 315339d (Update UI)
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
var redisConnection = builder.Configuration.GetConnectionString("Redis")
    ?? throw new InvalidOperationException("ConnectionStrings:Redis is required for shared output cache.");
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    var configuration = ConfigurationOptions.Parse(redisConnection);
    configuration.AbortOnConnectFail = false;
    configuration.ConnectTimeout = 1000;
    configuration.ConnectRetry = 0;
    return ConnectionMultiplexer.Connect(configuration);
});
builder.Services.AddStackExchangeRedisOutputCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = "culinaryblog:output:";
});
builder.Services
    .AddOutputCache(options =>
    {
        options.AddPolicy(
            "RecipeList",
            new RecipeListOutputCachePolicy());

        options.AddPolicy(
            "RecipeDetail",
            new RecipeDetailOutputCachePolicy());
        options.AddPolicy(
            "RecipeSearch",
            new RecipeSearchOutputCachePolicy());
        options.AddPolicy(
            "CategoryList",
            new CategoryOutputCachePolicy(TimeSpan.FromMinutes(30)));
        options.AddPolicy(
            "CategoryDetail",
            new CategoryOutputCachePolicy(TimeSpan.FromMinutes(5)));
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

builder.Services.AddHttpClient();
builder.Services.AddCulinaryTelemetry(builder.Configuration);
var healthChecks = builder.Services
    .AddHealthChecks()
    .AddCheck<PostgreSqlHealthCheck>(
        "postgresql",
        tags: ["ready"])
    .AddCheck<RedisHealthCheck>(
        "redis",
        tags: ["ready"]);
if (string.Equals(builder.Configuration["FileStorage:Provider"],
        "Minio", StringComparison.OrdinalIgnoreCase))
    healthChecks.AddCheck<MinioHealthCheck>(
        "minio", tags: ["dependency"]);

var app =
    builder.Build();

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnostics, http) =>
    {
        diagnostics.Set("Method", http.Request.Method);
        diagnostics.Set("Path", http.Request.Path.Value ?? "/");
        diagnostics.Set("StatusCode", http.Response.StatusCode);
        diagnostics.Set("CorrelationId",
            http.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString());
        diagnostics.Set("UserId", http.User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? http.User.FindFirst("sub")?.Value);
        diagnostics.Set("TraceId", System.Diagnostics.Activity.Current?.TraceId.ToString());
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
var staticContentTypes = new FileExtensionContentTypeProvider();
staticContentTypes.Mappings[".avif"] = "image/avif";
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = staticContentTypes
});

// Read the transactionally updated database epoch before a public cache lookup.
app.UseMiddleware<CacheEpochMiddleware>();
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
            .GetRequiredService<CulinaryBlogDbContext>();
    var passwordHasher =
        scope.ServiceProvider
            .GetRequiredService<IPasswordHasher<ApplicationUser>>();

    try
    {
        await DatabaseSeeder
            .SeedAsync(
                context,
                passwordHasher,
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
        context.AllowCacheLookup = isPublicRequest
            && !CacheEpochMiddleware.IsBypassed(context.HttpContext);
        context.AllowCacheStorage = context.AllowCacheLookup;
        context.ResponseExpirationTimeSpan =
            TimeSpan.FromMinutes(5);
        context.CacheVaryByRules.QueryKeys =
            new Microsoft.Extensions.Primitives.StringValues("*");
        context.CacheVaryByRules.HeaderNames =
            new Microsoft.Extensions.Primitives.StringValues(CacheEpochMiddleware.HeaderName);

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
        context.AllowCacheStorage =
            context.AllowCacheStorage
            && context.HttpContext.Response.StatusCode == StatusCodes.Status200OK
            && !context.HttpContext.Response.Headers.ContainsKey("Set-Cookie");
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
        context.AllowCacheLookup = isPublicRequest
            && !CacheEpochMiddleware.IsBypassed(context.HttpContext);
        context.AllowCacheStorage = context.AllowCacheLookup;
        context.ResponseExpirationTimeSpan =
            TimeSpan.FromMinutes(5);
        context.CacheVaryByRules.HeaderNames =
            new Microsoft.Extensions.Primitives.StringValues(CacheEpochMiddleware.HeaderName);

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
        context.AllowCacheStorage =
            context.AllowCacheStorage
            && context.HttpContext.Response.StatusCode == StatusCodes.Status200OK
            && !context.HttpContext.Response.Headers.ContainsKey("Set-Cookie");
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

internal sealed class RecipeSearchOutputCachePolicy : IOutputCachePolicy
{
    public ValueTask CacheRequestAsync(
        OutputCacheContext context, CancellationToken cancellationToken)
    {
        var anonymous = context.HttpContext.User.Identity?.IsAuthenticated != true;
        context.EnableOutputCaching = true;
        context.AllowCacheLookup = anonymous
            && !CacheEpochMiddleware.IsBypassed(context.HttpContext);
        context.AllowCacheStorage = context.AllowCacheLookup;
        context.ResponseExpirationTimeSpan = TimeSpan.FromMinutes(1);
        context.CacheVaryByRules.QueryKeys =
            new Microsoft.Extensions.Primitives.StringValues("*");
        context.CacheVaryByRules.HeaderNames =
            new Microsoft.Extensions.Primitives.StringValues(CacheEpochMiddleware.HeaderName);
        return ValueTask.CompletedTask;
    }

    public ValueTask ServeFromCacheAsync(
        OutputCacheContext context, CancellationToken cancellationToken) =>
        ValueTask.CompletedTask;

    public ValueTask ServeResponseAsync(
        OutputCacheContext context, CancellationToken cancellationToken)
    {
        context.AllowCacheStorage =
            context.AllowCacheStorage
            && context.HttpContext.Response.StatusCode == StatusCodes.Status200OK
            && !context.HttpContext.Response.Headers.ContainsKey("Set-Cookie");
        context.Tags.Add("recipes");
        return ValueTask.CompletedTask;
    }
}

internal sealed class CategoryOutputCachePolicy(TimeSpan expiration) : IOutputCachePolicy
{
    public ValueTask CacheRequestAsync(
        OutputCacheContext context, CancellationToken cancellationToken)
    {
        var anonymous = context.HttpContext.User.Identity?.IsAuthenticated != true;
        context.EnableOutputCaching = true;
        context.AllowCacheLookup = anonymous
            && !CacheEpochMiddleware.IsBypassed(context.HttpContext);
        context.AllowCacheStorage = context.AllowCacheLookup;
        context.ResponseExpirationTimeSpan = expiration;
        context.CacheVaryByRules.QueryKeys =
            new Microsoft.Extensions.Primitives.StringValues("*");
        context.CacheVaryByRules.HeaderNames =
            new Microsoft.Extensions.Primitives.StringValues(CacheEpochMiddleware.HeaderName);
        return ValueTask.CompletedTask;
    }

    public ValueTask ServeFromCacheAsync(
        OutputCacheContext context, CancellationToken cancellationToken) =>
        ValueTask.CompletedTask;

    public ValueTask ServeResponseAsync(
        OutputCacheContext context, CancellationToken cancellationToken)
    {
        context.AllowCacheStorage =
            context.AllowCacheStorage
            && context.HttpContext.Response.StatusCode == StatusCodes.Status200OK
            && !context.HttpContext.Response.Headers.ContainsKey("Set-Cookie");
        context.Tags.Add("categories");
        context.Tags.Add("recipes");
        return ValueTask.CompletedTask;
    }
}

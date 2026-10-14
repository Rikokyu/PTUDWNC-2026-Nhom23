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
using System.Threading.RateLimiting;

var builder =
    WebApplication.CreateBuilder(args);

var webRootPath = Path.Combine(
    builder.Environment.ContentRootPath,
    "wwwroot");
Directory.CreateDirectory(Path.Combine(webRootPath, "uploads"));
builder.Environment.WebRootPath = webRootPath;

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Application
builder.Services
    .AddApplicationServices();

builder.Services
    .AddJwtAuthentication(builder.Configuration);

builder.Services
    .AddFrontendCors(builder.Configuration);
builder.Services
    .AddApiRateLimiting();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = (context, _) =>
    {
        context.HttpContext.Response.Headers.RetryAfter = "60";
        return ValueTask.CompletedTask;
    };

    options.AddPolicy(
        "Auth",
        context => CreateIpPartition(context, 10));
    options.AddPolicy(
        "Api",
        context => CreateIpPartition(context, 100));
    options.AddPolicy(
        "Upload",
        context => CreateIpPartition(context, 5));
});

RateLimitPartition<string> CreateIpPartition(
    HttpContext context,
    int permitLimit) =>
    RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        });

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
            policy =>
            {
                policy
                    .Expire(
                        TimeSpan.FromMinutes(15))
                    .SetVaryByQuery("*")
                    .Tag("recipes");
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

        options.AddPolicy(
            "RecipeSlug",
            new RecipeSlugOutputCachePolicy());
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

var app =
    builder.Build();

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

app.UseHttpsRedirection();

app.UseRouting();
app.UseCors("Frontend");
app.UseRateLimiter();
app.UseAuthentication();
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

internal sealed class RecipeSlugOutputCachePolicy : IOutputCachePolicy
{
    public ValueTask CacheRequestAsync(
        OutputCacheContext context,
        CancellationToken cancellationToken) =>
        ValueTask.CompletedTask;

    public ValueTask ServeFromCacheAsync(
        OutputCacheContext context,
        CancellationToken cancellationToken) =>
        ValueTask.CompletedTask;

    public ValueTask ServeResponseAsync(
        OutputCacheContext context,
        CancellationToken cancellationToken)
    {
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
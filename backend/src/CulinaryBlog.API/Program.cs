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

app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

// Output cache
app.UseOutputCache();

app.UseHttpsRedirection();

app.UseCors("Frontend");
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
app.UseAuthorization();

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
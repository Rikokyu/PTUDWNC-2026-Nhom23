using CulinaryBlog.API.Extensions;
using CulinaryBlog.API.Middleware;
using CulinaryBlog.Infrastructure.Persistence.Seed;
using CulinaryBlog.Application;
using CulinaryBlog.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OutputCaching;

var builder =
    WebApplication.CreateBuilder(args);

// Application
builder.Services
    .AddApplicationServices();

// Infrastructure
builder.Services
    .AddInfrastructure(
        builder.Configuration);

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

builder.Services
    .AddSwaggerGen();

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
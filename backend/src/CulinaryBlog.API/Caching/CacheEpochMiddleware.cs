using System.Globalization;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace CulinaryBlog.API.Caching;

public sealed class CacheEpochMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Culinary-Cache-Epoch";
    public const string BypassItemKey = "CulinaryBlog.BypassOutputCache";

    public async Task InvokeAsync(
        HttpContext context,
        CulinaryBlogDbContext dbContext,
        IConnectionMultiplexer redis)
    {
        // Never accept a caller-supplied epoch as cache key material.
        context.Request.Headers.Remove(HeaderName);
        if (!HttpMethods.IsGet(context.Request.Method)
            || context.User.Identity?.IsAuthenticated == true
            || !IsPublicCachePath(context.Request.Path))
        {
            await next(context);
            return;
        }

        if (!redis.IsConnected)
        {
            context.Items[BypassItemKey] = true;
            await next(context);
            return;
        }

        try
        {
            var epoch = await dbContext.Database.SqlQueryRaw<long>(
                """SELECT "Version" AS "Value" FROM "CacheEpochs" WHERE "Id" = 1""")
                .SingleAsync(context.RequestAborted);
            context.Request.Headers[HeaderName] =
                epoch.ToString(CultureInfo.InvariantCulture);
        }
        catch (Exception) when (!context.RequestAborted.IsCancellationRequested)
        {
            // The database may be unavailable or the cache migration missing.
            // Bypass the cache so an old response is never served as current.
            context.Items[BypassItemKey] = true;
        }

        await next(context);
    }

    public static bool IsBypassed(HttpContext context) =>
        context.Items.ContainsKey(BypassItemKey);

    private static bool IsPublicCachePath(PathString path) =>
        path.StartsWithSegments("/api/v1/categories")
        || path.StartsWithSegments("/api/v1/recipes");
}

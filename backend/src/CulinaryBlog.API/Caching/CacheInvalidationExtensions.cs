using Microsoft.AspNetCore.OutputCaching;
using Serilog;

namespace CulinaryBlog.API.Caching;

public static class CacheInvalidationExtensions
{
    public static async ValueTask SafeEvictByTagAsync(
        this IOutputCacheStore cache, string tag, CancellationToken cancellationToken)
    {
        try
        {
            await cache.EvictByTagAsync(tag, cancellationToken);
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            // The PostgreSQL cache epoch changes in the mutation transaction.
            // Old entries remain stored but can no longer match the cache key.
            Log.Warning("Redis cache eviction unavailable; the database cache epoch prevents stale reads.");
        }
    }
}

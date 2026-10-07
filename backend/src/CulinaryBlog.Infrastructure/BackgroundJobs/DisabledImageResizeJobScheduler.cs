using CulinaryBlog.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Infrastructure.BackgroundJobs;

public sealed class DisabledImageResizeJobScheduler(
    ILogger<DisabledImageResizeJobScheduler> logger)
    : IImageResizeJobScheduler
{
    public void Enqueue(Guid imageId)
    {
        logger.LogWarning(
            "Image resize job {ImageId} was not queued because Hangfire is disabled.",
            imageId);
    }
}

using CulinaryBlog.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Infrastructure.BackgroundJobs;

public sealed class DisabledImageCleanupJobScheduler(
    ILogger<DisabledImageCleanupJobScheduler> logger)
    : IImageCleanupJobScheduler
{
    public void Enqueue(string fileUrl)
    {
        logger.LogWarning(
            "Image file {FileUrl} was not queued for cleanup because Hangfire is disabled.",
            fileUrl);
    }
}

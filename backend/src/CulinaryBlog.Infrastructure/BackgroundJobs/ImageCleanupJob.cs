using CulinaryBlog.Application.Common.Interfaces;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Infrastructure.BackgroundJobs;

public sealed class ImageCleanupJob(
    IFileStorageService fileStorage,
    ILogger<ImageCleanupJob> logger)
{
    [AutomaticRetry(Attempts = 3)]
    public async Task DeleteAsync(
        string fileUrl,
        CancellationToken cancellationToken)
    {
        await fileStorage.DeleteAsync(fileUrl, cancellationToken);
        logger.LogInformation(
            "Deleted recipe image file {FileUrl}.",
            fileUrl);
    }
}

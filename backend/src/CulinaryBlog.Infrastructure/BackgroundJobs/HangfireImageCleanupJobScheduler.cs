using CulinaryBlog.Application.Common.Interfaces;
using Hangfire;

namespace CulinaryBlog.Infrastructure.BackgroundJobs;

public sealed class HangfireImageCleanupJobScheduler(
    IBackgroundJobClient backgroundJobs) : IImageCleanupJobScheduler
{
    public void Enqueue(string fileUrl)
    {
        backgroundJobs.Enqueue<ImageCleanupJob>(
            job => job.DeleteAsync(fileUrl, CancellationToken.None));
    }
}

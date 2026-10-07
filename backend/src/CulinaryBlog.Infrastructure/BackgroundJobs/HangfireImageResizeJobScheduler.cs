using CulinaryBlog.Application.Common.Interfaces;
using Hangfire;

namespace CulinaryBlog.Infrastructure.BackgroundJobs;

public sealed class HangfireImageResizeJobScheduler(
    IBackgroundJobClient backgroundJobs) : IImageResizeJobScheduler
{
    public void Enqueue(Guid imageId)
    {
        backgroundJobs.Enqueue<ImageResizeJob>(
            job => job.ProcessAsync(imageId, CancellationToken.None));
    }
}

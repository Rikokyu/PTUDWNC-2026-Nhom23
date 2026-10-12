using CulinaryBlog.Application.Common.Interfaces;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Infrastructure.BackgroundJobs;

public sealed class HangfireWelcomeEmailJobScheduler(
    IBackgroundJobClient backgroundJobs,
    ILogger<HangfireWelcomeEmailJobScheduler> logger)
    : IWelcomeEmailJobScheduler
{
    public void Enqueue(string email, string fullName)
    {
        var jobId = backgroundJobs.Enqueue<WelcomeEmailJob>(
            job => job.SendAsync(email, fullName, null!));

        logger.LogInformation(
            "Enqueued welcome email job {JobId} for {Email}.",
            jobId,
            email);
    }
}

using CulinaryBlog.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Infrastructure.BackgroundJobs;

public sealed class DisabledWelcomeEmailJobScheduler(
    ILogger<DisabledWelcomeEmailJobScheduler> logger)
    : IWelcomeEmailJobScheduler
{
    public void Enqueue(string email, string displayName)
    {
        logger.LogWarning(
            "Welcome email for {Email} was not queued because Hangfire is disabled.",
            email);
    }
}

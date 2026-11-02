using CulinaryBlog.Application.Common.Interfaces;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Infrastructure.BackgroundJobs;

public sealed class WelcomeEmailJob(
    IEmailService emailService,
    ILogger<WelcomeEmailJob> logger)
{
    private const int MaximumRetryCount = 3;

    [AutomaticRetry(
        Attempts = MaximumRetryCount,
        DelaysInSeconds = [60, 300, 1800],
        OnAttemptsExceeded = AttemptsExceededAction.Fail,
        LogEvents = true)]
    public async Task SendAsync(
        string email,
        string displayName,
        PerformContext? context)
    {
        try
        {
            await emailService.SendWelcomeEmailAsync(email, displayName);
        }
        catch (Exception exception)
        {
            var retryCount = context?.GetJobParameter<int>("RetryCount") ?? 0;

            if (retryCount >= MaximumRetryCount)
            {
                logger.LogError(
                    exception,
                    "Welcome email to {Email} failed permanently after {RetryCount} retries.",
                    email,
                    MaximumRetryCount);
            }
            else
            {
                logger.LogWarning(
                    exception,
                    "Welcome email to {Email} failed. Hangfire retry {NextRetry}/{MaximumRetryCount} will be scheduled.",
                    email,
                    retryCount + 1,
                    MaximumRetryCount);
            }

            throw;
        }
    }
}

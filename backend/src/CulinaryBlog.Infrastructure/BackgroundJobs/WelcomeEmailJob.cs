
using Hangfire;
using CulinaryBlog.Application.Common.Interfaces;

namespace CulinaryBlog.Infrastructure.BackgroundJobs;

public sealed class WelcomeEmailJob
{
    private readonly IEmailService _emailService;

    public WelcomeEmailJob(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [AutomaticRetry(
        Attempts = 3,
        DelaysInSeconds = new int[] { 60, 300, 1800 },
        OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public async Task ExecuteAsync(
        string email,
        string displayName)
    {
        await _emailService.SendWelcomeEmailAsync(
            email,
            displayName);
    }
}

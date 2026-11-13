using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Infrastructure.BackgroundJobs;
using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.States;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CulinaryBlog.Application.Tests;

public sealed class WelcomeEmailHangfirePostgresTests
{
    [Fact]
    public async Task IsolatedPostgres_QueueRetriesThenFails()
    {
        var connectionString = Environment.GetEnvironmentVariable("CULINARY_JOBS_TEST_DB");
        if (connectionString is null) return;
        var email = new AlwaysFailEmail();
        GlobalConfiguration.Configuration
            .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString))
            .UseActivator(new FailingActivator(email))
            .UseFilter(new WelcomeEmailFailureFilter(
                NullLogger<WelcomeEmailFailureFilter>.Instance));

        using var server = new BackgroundJobServer(new BackgroundJobServerOptions
        {
            WorkerCount = 1,
            SchedulePollingInterval = TimeSpan.FromMilliseconds(100)
        });
        var client = new BackgroundJobClient();
        var jobId = client.Enqueue<WelcomeEmailJob>(job =>
            job.ExecuteAsync("failure@culinary.test", "Failure Test"));
        var until = DateTime.UtcNow.AddSeconds(25);
        string? state = null;
        while (DateTime.UtcNow < until)
        {
            using var connection = JobStorage.Current.GetConnection();
            state = connection.GetStateData(jobId)?.Name;
            if (state == FailedState.StateName) break;
            if (state == ScheduledState.StateName)
                client.ChangeState(jobId, new EnqueuedState(), ScheduledState.StateName);
            await Task.Delay(100);
        }
        Assert.Equal(FailedState.StateName, state);
        Assert.Equal(4, email.Attempts);
    }

    private sealed class FailingActivator(AlwaysFailEmail email) : JobActivator
    {
        public override object ActivateJob(Type jobType) =>
            jobType == typeof(WelcomeEmailJob)
                ? new WelcomeEmailJob(email)
                : base.ActivateJob(jobType);
    }

    private sealed class AlwaysFailEmail : IEmailService
    {
        public int Attempts;
        public Task SendWelcomeEmailAsync(string email, string displayName,
            CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref Attempts);
            throw new InvalidOperationException("Synthetic SMTP failure");
        }

        public Task SendPasswordResetEmailAsync(string email, string displayName,
            string resetUrl, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}

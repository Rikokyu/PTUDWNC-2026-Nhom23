using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Infrastructure.BackgroundJobs;

public sealed class WelcomeEmailFailureFilter(
    ILogger<WelcomeEmailFailureFilter> logger) : JobFilterAttribute, IApplyStateFilter
{
    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        if (context.NewState is FailedState failed
            && context.BackgroundJob.Job.Type == typeof(WelcomeEmailJob))
        {
            logger.LogError(failed.Exception,
                "Welcome email job {JobId} failed after retries.",
                context.BackgroundJob.Id);
        }
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
    }
}

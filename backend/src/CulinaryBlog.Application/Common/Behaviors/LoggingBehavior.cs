using System.Diagnostics;
using CulinaryBlog.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Application.Common.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger,
    ICurrentUser currentUser)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const long SlowRequestThresholdMilliseconds = 500;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var succeeded = false;

        try
        {
            var response = await next(cancellationToken);
            succeeded = true;
            return response;
        }
        finally
        {
            stopwatch.Stop();
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            var requestType = typeof(TRequest).Name;
            var userId = currentUser.UserId;

            if (elapsedMilliseconds > SlowRequestThresholdMilliseconds)
            {
                logger.LogWarning(
                    "Handled request {RequestType} for user {UserId} in {ElapsedMilliseconds} ms. Succeeded: {Succeeded}.",
                    requestType,
                    userId,
                    elapsedMilliseconds,
                    succeeded);
            }
            else if (!succeeded)
            {
                logger.LogError(
                    "Request {RequestType} for user {UserId} failed after {ElapsedMilliseconds} ms.",
                    requestType,
                    userId,
                    elapsedMilliseconds);
            }
            else
            {
                logger.LogInformation(
                    "Handled request {RequestType} for user {UserId} in {ElapsedMilliseconds} ms.",
                    requestType,
                    userId,
                    elapsedMilliseconds);
            }
        }
    }
}
using System.Diagnostics;
using CulinaryBlog.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Application.Common.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse> :
    IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUser _currentUser;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        ICurrentUser currentUser)
    {
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();
            stopwatch.Stop();

            var logLevel = stopwatch.ElapsedMilliseconds >= 500
                ? LogLevel.Warning
                : LogLevel.Information;

            _logger.Log(
                logLevel,
                "MediatR request {RequestName} completed in {ElapsedMilliseconds} ms for user {UserId}.",
                requestName,
                stopwatch.ElapsedMilliseconds,
                _currentUser.UserId);

            return response;
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            _logger.LogError(
                exception,
                "MediatR request {RequestName} failed in {ElapsedMilliseconds} ms for user {UserId}.",
                requestName,
                stopwatch.ElapsedMilliseconds,
                _currentUser.UserId);
            throw;
        }
    }
}
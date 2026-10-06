using System.Diagnostics;
using CulinaryBlog.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Application.Common.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
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
		var userId = _currentUser.UserId?.ToString();
		using var scope = _logger.BeginScope(new Dictionary<string, object?>
		{
			["UserId"] = userId ?? "anonymous",
			["RequestType"] = typeof(TRequest).Name
		});

		var stopwatch = Stopwatch.StartNew();
		_logger.LogInformation("Handling application request {RequestType}", typeof(TRequest).Name);

		try
		{
			var response = await next();
			stopwatch.Stop();

			if (stopwatch.ElapsedMilliseconds > 500)
			{
				_logger.LogWarning(
					"Application request {RequestType} exceeded performance threshold in {ElapsedMilliseconds} ms",
					typeof(TRequest).Name,
					stopwatch.ElapsedMilliseconds);
			}
			else
			{
				_logger.LogInformation(
					"Handled application request {RequestType} in {ElapsedMilliseconds} ms",
					typeof(TRequest).Name,
					stopwatch.ElapsedMilliseconds);
			}

			return response;
		}
		catch (Exception exception)
		{
			stopwatch.Stop();
			_logger.LogError(
				exception,
				"Application request {RequestType} failed in {ElapsedMilliseconds} ms",
				typeof(TRequest).Name,
				stopwatch.ElapsedMilliseconds);
			throw;
		}
	}
}

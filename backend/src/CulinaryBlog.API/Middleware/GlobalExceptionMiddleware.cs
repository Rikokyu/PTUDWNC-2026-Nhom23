using Microsoft.AspNetCore.Mvc;

namespace CulinaryBlog.API.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException)
            when (context.RequestAborted.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Request {CorrelationId} was cancelled by the client.",
                context.TraceIdentifier);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unhandled error for request {CorrelationId}.",
                context.TraceIdentifier);

            if (context.Response.HasStarted)
                throw;

            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Đã xảy ra lỗi máy chủ",
                Detail = _environment.IsDevelopment()
                    ? exception.Message
                    : "Vui lòng thử lại sau.",
                Instance = context.Request.Path
            };
            problem.Extensions["correlationId"] =
                context.TraceIdentifier;

            context.Response.StatusCode = problem.Status.Value;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}

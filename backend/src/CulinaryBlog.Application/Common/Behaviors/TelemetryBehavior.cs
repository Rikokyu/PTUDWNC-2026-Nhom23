using System.Diagnostics;
using System.Diagnostics.Metrics;
using MediatR;
using Microsoft.Extensions.Logging;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.DTOs.Recipes;

namespace CulinaryBlog.Application.Common.Behaviors;

public static class CulinaryTelemetry
{
    public const string Name = "CulinaryBlog";
    public static readonly ActivitySource Source = new(Name);
    public static readonly Meter Meter = new(Name);
    public static readonly Counter<long> Searches = Meter.CreateCounter<long>("culinary.recipe.searches", "{search}");
    public static readonly Counter<long> EmptySearches = Meter.CreateCounter<long>("culinary.recipe.searches.empty", "{search}");
    public static readonly Counter<long> Changes = Meter.CreateCounter<long>("culinary.recipe.changes", "{recipe}");
    public static readonly Counter<long> Uploads = Meter.CreateCounter<long>("culinary.image.uploads", "{upload}");
    public static readonly Histogram<double> Duration = Meter.CreateHistogram<double>("culinary.operation.duration", "s");
}

// Only operation names and bounded outcomes are recorded, never request payloads.
public sealed class TelemetryBehavior<TRequest, TResponse>(ILogger<TelemetryBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var operation = typeof(TRequest).Name;
        using var activity = CulinaryTelemetry.Source.StartActivity(operation);
        var started = Stopwatch.GetTimestamp();
        var outcome = "success";
        try
        {
            var response = await next();
            if (operation == "SearchRecipesQuery")
            {
                CulinaryTelemetry.Searches.Add(1);
                if (response is PagedResult<RecipeSummaryDto> { TotalCount: 0 }) CulinaryTelemetry.EmptySearches.Add(1);
            }
            if (operation is "CreateRecipeCommand")
                CulinaryTelemetry.Changes.Add(1, new KeyValuePair<string, object?>("operation", operation));
            if (operation == "UploadRecipeImageCommand")
                CulinaryTelemetry.Uploads.Add(1, new KeyValuePair<string, object?>("outcome", "success"));
            if (operation.EndsWith("Command", StringComparison.Ordinal)) logger.LogInformation("Application operation {Operation} completed", operation);
            return response;
        }
        catch (Exception)
        {
            outcome = "failure";
            activity?.SetStatus(ActivityStatusCode.Error);
            if (operation == "UploadRecipeImageCommand")
                CulinaryTelemetry.Uploads.Add(1, new KeyValuePair<string, object?>("outcome", "failure"));
            logger.LogWarning("Application operation {Operation} failed", operation);
            throw;
        }
        finally
        {
            CulinaryTelemetry.Duration.Record(Stopwatch.GetElapsedTime(started).TotalSeconds,
                new KeyValuePair<string, object?>("operation", operation), new KeyValuePair<string, object?>("outcome", outcome));
        }
    }
}

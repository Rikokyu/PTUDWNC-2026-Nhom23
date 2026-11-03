using System.Diagnostics;
using System.Diagnostics.Metrics;
using CulinaryBlog.Application.Common.Behaviors;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.DTOs.Recipes;
using CulinaryBlog.Application.Features.Recipes.Queries.SearchRecipes;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CulinaryBlog.Application.Tests;

public sealed class TelemetryBehaviorTests
{
    [Fact]
    public async Task SearchRecordsChildSpanAndEmptyResultMetricsWithoutKeyword()
    {
        var activities = new List<Activity>();
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == CulinaryTelemetry.Name,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity => activities.Add(activity)
        };
        ActivitySource.AddActivityListener(listener);
        var measurements = new List<string>();
        using var meter = new MeterListener();
        meter.InstrumentPublished = (instrument, l) => { if (instrument.Meter.Name == CulinaryTelemetry.Name) l.EnableMeasurementEvents(instrument); };
        meter.SetMeasurementEventCallback<long>((instrument, _, tags, _) =>
        {
            lock (measurements) measurements.Add(instrument.Name);
            foreach (var tag in tags) Assert.DoesNotContain("secret", tag.Value?.ToString() ?? "");
        });
        meter.Start();
        using var parent = new Activity("parent").Start();
        var behavior = new TelemetryBehavior<SearchRecipesQuery, PagedResult<RecipeSummaryDto>>(
            NullLogger<TelemetryBehavior<SearchRecipesQuery, PagedResult<RecipeSummaryDto>>>.Instance);
        await behavior.Handle(new SearchRecipesQuery("secret-keyword", 1, 12),
            _ => Task.FromResult(new PagedResult<RecipeSummaryDto>([], 0, 1, 12)), default);
        var span = Assert.Single(activities, a => a.OperationName == "SearchRecipesQuery");
        Assert.Equal(parent.TraceId, span.TraceId);
        Assert.Equal(parent.SpanId, span.ParentSpanId);
        Assert.Empty(span.Tags);
        Assert.Contains("culinary.recipe.searches", measurements);
        Assert.Contains("culinary.recipe.searches.empty", measurements);
    }

    [Fact]
    public async Task FailedSearchDoesNotRecordSuccessfulSearch()
    {
        var behavior = new TelemetryBehavior<SearchRecipesQuery, PagedResult<RecipeSummaryDto>>(
            NullLogger<TelemetryBehavior<SearchRecipesQuery, PagedResult<RecipeSummaryDto>>>.Instance);
        await Assert.ThrowsAsync<InvalidOperationException>(() => behavior.Handle(
            new SearchRecipesQuery("valid", 1, 12), _ => throw new InvalidOperationException(), default));
    }

    [Theory]
    [InlineData(0, 1, 12, 0, false, false)]
    [InlineData(25, 1, 12, 3, true, false)]
    [InlineData(25, 2, 12, 3, true, true)]
    [InlineData(25, 3, 12, 3, false, true)]
    [InlineData(25, 4, 12, 3, false, true)]
    public void PaginationMetadata(int count, int page, int size, int pages, bool next, bool previous)
    {
        var result = new PagedResult<int>([], count, page, size);
        Assert.Equal(pages, result.TotalPages);
        Assert.Equal(next, result.HasNextPage);
        Assert.Equal(previous, result.HasPreviousPage);
    }
}

using System.Diagnostics.Metrics;

namespace CulinaryBlog.Application.Common.Observability;

public static class RecipeMetrics
{
    public const string MeterName = "CulinaryBlog.Recipes";
    private static readonly Meter Meter = new(MeterName);
    private static readonly Counter<long> Created =
        Meter.CreateCounter<long>("culinaryblog.recipe.created");
    private static readonly Counter<long> Published =
        Meter.CreateCounter<long>("culinaryblog.recipe.published");

    public static void RecordCreated() => Created.Add(1);
    public static void RecordPublished() => Published.Add(1);
}

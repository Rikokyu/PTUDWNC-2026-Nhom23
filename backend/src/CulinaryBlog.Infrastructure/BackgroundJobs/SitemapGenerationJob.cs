using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Xml;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Infrastructure.Persistence;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Infrastructure.BackgroundJobs;

public sealed class SitemapGenerationJob(
    CulinaryBlogDbContext dbContext,
    string webRootPath,
    string baseUrl,
    string[] staticPaths,
    ILogger<SitemapGenerationJob> logger)
{
    public const string RecurringJobId = "sitemap-generation";
    public const string CronExpression = "0 2 * * *";
    public static TimeZoneInfo ScheduleTimeZone => TimeZoneInfo.Utc;

    [AutomaticRetry(
        Attempts = 2,
        OnAttemptsExceeded = AttemptsExceededAction.Fail,
        LogEvents = true)]
    public async Task GenerateAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var siteUri)
                || siteUri.Scheme is not ("http" or "https"))
            {
                throw new InvalidOperationException(
                    "Sitemap:BaseUrl must be an absolute HTTP or HTTPS URL.");
            }

            var categories = await dbContext.Categories
                .AsNoTracking()
                .Where(category => !category.IsDeleted)
                .OrderBy(category => category.Slug)
                .Select(category => new SitemapPage(
                    category.Slug,
                    category.UpdatedAt ?? category.CreatedAt))
                .ToListAsync(cancellationToken);

            var recipes = await dbContext.Recipes
                .AsNoTracking()
                .Where(recipe => !recipe.IsDeleted
                    && recipe.Status == RecipeStatus.Published
                    && !recipe.Category.IsDeleted)
                .OrderBy(recipe => recipe.Slug)
                .Select(recipe => new SitemapPage(
                    recipe.Slug,
                    recipe.UpdatedAt ?? recipe.CreatedAt))
                .ToListAsync(cancellationToken);

            var entries = BuildEntries(
                siteUri,
                staticPaths,
                categories,
                recipes);

            Directory.CreateDirectory(webRootPath);
            await WriteSitemapAtomicallyAsync(
                Path.Combine(webRootPath, "sitemap.xml"),
                entries,
                cancellationToken);
            await UpdateRobotsTxtAtomicallyAsync(
                webRootPath,
                siteUri,
                cancellationToken);

            stopwatch.Stop();
            logger.LogInformation(
                "Generated sitemap with {UrlCount} URLs ({RecipeCount} published recipes, {CategoryCount} categories, {StaticPageCount} static pages) in {ElapsedMilliseconds} ms.",
                entries.Count,
                recipes.Count,
                categories.Count,
                staticPaths.Length,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            logger.LogError(
                exception,
                "Sitemap generation failed after {ElapsedMilliseconds} ms.",
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public static IReadOnlyList<SitemapEntry> BuildEntries(
        Uri siteUri,
        IEnumerable<string> staticPaths,
        IEnumerable<SitemapPage> categoryPages,
        IEnumerable<SitemapPage> recipePages)
    {
        var entries = staticPaths.Select(path => new SitemapEntry(
                BuildAbsoluteUrl(siteUri, path),
                LastModifiedUtc: null))
            .Concat(categoryPages.Select(page => new SitemapEntry(
                BuildAbsoluteUrl(
                    siteUri,
                    $"/categories/{Uri.EscapeDataString(page.Slug)}"),
                page.LastModifiedUtc is { } categoryModified
                    ? NormalizeUtc(categoryModified)
                    : null)))
            .Concat(recipePages.Select(page => new SitemapEntry(
                BuildAbsoluteUrl(
                    siteUri,
                    $"/recipes/{Uri.EscapeDataString(page.Slug)}"),
                page.LastModifiedUtc is { } recipeModified
                    ? NormalizeUtc(recipeModified)
                    : null)));

        return entries
            .GroupBy(entry => entry.Url, StringComparer.Ordinal)
            .Select(group => new SitemapEntry(
                group.Key,
                group.Where(entry => entry.LastModifiedUtc.HasValue)
                    .Select(entry => entry.LastModifiedUtc)
                    .Max()))
            .OrderBy(entry => entry.Url, StringComparer.Ordinal)
            .ToArray();
    }

    public static IReadOnlyList<string> BuildUrls(
        Uri siteUri,
        IEnumerable<string> staticPaths,
        IEnumerable<string> categorySlugs,
        IEnumerable<string> recipeSlugs)
    {
        var categories = categorySlugs.Select(slug =>
            new SitemapPage(slug, LastModifiedUtc: null));
        var recipes = recipeSlugs.Select(slug =>
            new SitemapPage(slug, LastModifiedUtc: null));

        return BuildEntries(siteUri, staticPaths, categories, recipes)
            .Select(entry => entry.Url)
            .ToArray();
    }

    private static DateTime NormalizeUtc(DateTime dateTime) =>
        dateTime.Kind switch
        {
            DateTimeKind.Utc => dateTime,
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
        };

    private static string BuildAbsoluteUrl(Uri siteUri, string path)
    {
        var normalizedPath = path.StartsWith('/') ? path : $"/{path}";
        return new Uri(siteUri, normalizedPath).AbsoluteUri;
    }

    private static async Task WriteSitemapAtomicallyAsync(
        string sitemapPath,
        IReadOnlyCollection<SitemapEntry> entries,
        CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(sitemapPath)!;
        var temporaryPath = Path.Combine(
            directory,
            $"sitemap.{Guid.NewGuid():N}.tmp");

        try
        {
            await using (var stream = new FileStream(
                temporaryPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                4096,
                useAsync: true))
            await using (var writer = XmlWriter.Create(stream, new XmlWriterSettings
            {
                Async = true,
                Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                Indent = true
            }))
            {
                const string sitemapNamespace =
                    "http://www.sitemaps.org/schemas/sitemap/0.9";

                await writer.WriteStartDocumentAsync();
                await writer.WriteStartElementAsync(
                    null,
                    "urlset",
                    sitemapNamespace);

                foreach (var entry in entries)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await writer.WriteStartElementAsync(null, "url", null);
                    await writer.WriteElementStringAsync(
                        null,
                        "loc",
                        null,
                        entry.Url);

                    if (entry.LastModifiedUtc is { } lastModifiedUtc)
                    {
                        await writer.WriteElementStringAsync(
                            null,
                            "lastmod",
                            null,
                            lastModifiedUtc.ToString(
                                "yyyy-MM-dd",
                                CultureInfo.InvariantCulture));
                    }

                    await writer.WriteEndElementAsync();
                }

                await writer.WriteEndElementAsync();
                await writer.WriteEndDocumentAsync();
                await writer.FlushAsync();
            }

            File.Move(temporaryPath, sitemapPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    private static async Task UpdateRobotsTxtAtomicallyAsync(
        string root,
        Uri siteUri,
        CancellationToken cancellationToken)
    {
        var robotsPath = Path.Combine(root, "robots.txt");
        var lines = File.Exists(robotsPath)
            ? (await File.ReadAllLinesAsync(robotsPath, cancellationToken))
                .Where(line => !line.TrimStart().StartsWith(
                    "Sitemap:",
                    StringComparison.OrdinalIgnoreCase))
                .ToList()
            : [];

        lines.Add($"Sitemap: {new Uri(siteUri, "/sitemap.xml").AbsoluteUri}");

        var temporaryPath = Path.Combine(
            root,
            $"robots.{Guid.NewGuid():N}.tmp");
        try
        {
            await File.WriteAllLinesAsync(
                temporaryPath,
                lines,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                cancellationToken);
            File.Move(temporaryPath, robotsPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }
}

public sealed record SitemapPage(string Slug, DateTime? LastModifiedUtc);

public sealed record SitemapEntry(string Url, DateTime? LastModifiedUtc);

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
    [AutomaticRetry(
        Attempts = 2,
        OnAttemptsExceeded = AttemptsExceededAction.Fail,
        LogEvents = true)]
    public async Task GenerateAsync(CancellationToken cancellationToken)
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
            .Select(category => category.Slug)
            .ToListAsync(cancellationToken);

        var recipes = await dbContext.Recipes
            .AsNoTracking()
            .Where(recipe => !recipe.IsDeleted
                && recipe.Status == RecipeStatus.Published
                && !recipe.Category.IsDeleted)
            .OrderBy(recipe => recipe.Slug)
            .Select(recipe => recipe.Slug)
            .ToListAsync(cancellationToken);

        var urls = BuildUrls(siteUri, staticPaths, categories, recipes);
        Directory.CreateDirectory(webRootPath);

        var sitemapPath = Path.Combine(webRootPath, "sitemap.xml");
        var temporaryPath = Path.Combine(
            webRootPath,
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
                await writer.WriteStartDocumentAsync();
                await writer.WriteStartElementAsync(
                    null,
                    "urlset",
                    "http://www.sitemaps.org/schemas/sitemap/0.9");
                foreach (var url in urls)
                {
                    await writer.WriteStartElementAsync(null, "url", null);
                    await writer.WriteElementStringAsync(null, "loc", null, url);
                    await writer.WriteEndElementAsync();
                }

                await writer.WriteEndElementAsync();
                await writer.WriteEndDocumentAsync();
            }

            File.Move(temporaryPath, sitemapPath, overwrite: true);
            await UpdateRobotsTxtAsync(webRootPath, siteUri, cancellationToken);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }

        logger.LogInformation(
            "Generated sitemap at {SitemapPath} with {UrlCount} URLs ({RecipeCount} published recipes, {CategoryCount} categories, {StaticPageCount} static pages).",
            sitemapPath,
            urls.Count,
            recipes.Count,
            categories.Count,
            staticPaths.Length);
    }

    public static IReadOnlyList<string> BuildUrls(
        Uri siteUri,
        IEnumerable<string> staticPaths,
        IEnumerable<string> categorySlugs,
        IEnumerable<string> recipeSlugs)
    {
        var paths = staticPaths
            .Concat(categorySlugs.Select(slug => $"/categories/{Uri.EscapeDataString(slug)}"))
            .Concat(recipeSlugs.Select(slug => $"/recipes/{Uri.EscapeDataString(slug)}"));

        return paths
            .Select(path => BuildAbsoluteUrl(siteUri, path))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static string BuildAbsoluteUrl(Uri siteUri, string path)
    {
        var normalizedPath = path.StartsWith('/') ? path : $"/{path}";
        return new Uri(siteUri, normalizedPath).AbsoluteUri;
    }

    private static async Task UpdateRobotsTxtAsync(
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
        await File.WriteAllLinesAsync(robotsPath, lines, cancellationToken);
    }
}

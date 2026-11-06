using System.Xml.Linq;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Infrastructure.BackgroundJobs;
using CulinaryBlog.Infrastructure.Persistence;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CulinaryBlog.Application.Tests;

public sealed class SitemapGenerationJobTests
{
    private static readonly XNamespace SitemapNamespace =
        "http://www.sitemaps.org/schemas/sitemap/0.9";

    [Fact]
    public void Schedule_UsesOneStableIdAndDailyUtcCron()
    {
        Assert.Equal("sitemap-generation", SitemapGenerationJob.RecurringJobId);
        Assert.Equal("0 2 * * *", SitemapGenerationJob.CronExpression);
        Assert.Equal(TimeZoneInfo.Utc, SitemapGenerationJob.ScheduleTimeZone);
    }

    [Fact]
    public void RetryPolicy_RetriesTwiceThenMovesJobToFailed()
    {
        var retry = typeof(SitemapGenerationJob)
            .GetMethod(nameof(SitemapGenerationJob.GenerateAsync))!
            .GetCustomAttributes(typeof(AutomaticRetryAttribute), inherit: true)
            .Cast<AutomaticRetryAttribute>()
            .Single();

        Assert.Equal(2, retry.Attempts);
        Assert.Equal(AttemptsExceededAction.Fail, retry.OnAttemptsExceeded);
        Assert.True(retry.LogEvents);
    }

    [Fact]
    public async Task GenerateAsync_WritesValidXmlWithOnlyPublicPagesAndUpdatesRobots()
    {
        var options = new DbContextOptionsBuilder<CulinaryBlogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var outputDirectory = Path.Combine(
            Path.GetTempPath(),
            $"sitemap-job-tests-{Guid.NewGuid():N}");
        var createdAt = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);
        var updatedAt = new DateTime(2026, 10, 4, 0, 0, 0, DateTimeKind.Utc);

        try
        {
            await using var dbContext = new CulinaryBlogDbContext(options);
            var category = CreateCategory("Main Dishes", "main-dishes", createdAt);
            var deletedCategory = CreateCategory("Deleted", "deleted", createdAt);
            deletedCategory.IsDeleted = true;
            dbContext.Categories.AddRange(category, deletedCategory);
            dbContext.Recipes.AddRange(
                CreateRecipe("published&safe", category, RecipeStatus.Published, createdAt, updatedAt),
                CreateRecipe("draft-hidden", category, RecipeStatus.Draft, createdAt),
                CreateRecipe("archived-hidden", category, RecipeStatus.Archived, createdAt),
                CreateRecipe("deleted-hidden", category, RecipeStatus.Published, createdAt, isDeleted: true),
                CreateRecipe("deleted-category-hidden", deletedCategory, RecipeStatus.Published, createdAt));
            await dbContext.SaveChangesAsync();

            Directory.CreateDirectory(outputDirectory);
            await File.WriteAllTextAsync(
                Path.Combine(outputDirectory, "robots.txt"),
                "User-agent: *\nDisallow: /private\nSitemap: https://old.example/sitemap.xml\n");

            var job = new SitemapGenerationJob(
                dbContext,
                outputDirectory,
                "https://culinary.example",
                ["/", "/recipes", "/categories", "/search?tag=a&sort=b", "/categories/main-dishes"],
                NullLogger<SitemapGenerationJob>.Instance);

            await job.GenerateAsync(CancellationToken.None);

            var sitemapPath = Path.Combine(outputDirectory, "sitemap.xml");
            var sitemapText = await File.ReadAllTextAsync(sitemapPath);
            var document = XDocument.Parse(sitemapText);
            var urls = document.Root!
                .Elements(SitemapNamespace + "url")
                .ToArray();
            var locations = urls
                .Select(url => (string)url.Element(SitemapNamespace + "loc")!)
                .ToArray();

            Assert.Equal(locations.Length, locations.Distinct(StringComparer.Ordinal).Count());
            Assert.Contains("https://culinary.example/", locations);
            Assert.Contains("https://culinary.example/categories/main-dishes", locations);
            Assert.Contains("https://culinary.example/recipes/published%26safe", locations);
            Assert.Contains("https://culinary.example/search?tag=a&sort=b", locations);
            Assert.DoesNotContain(locations, url => url.Contains("localhost", StringComparison.Ordinal));
            Assert.DoesNotContain(locations, url => url.Contains("draft-hidden", StringComparison.Ordinal));
            Assert.DoesNotContain(locations, url => url.Contains("archived-hidden", StringComparison.Ordinal));
            Assert.DoesNotContain(locations, url => url.Contains("deleted-hidden", StringComparison.Ordinal));
            Assert.DoesNotContain(locations, url => url.Contains("deleted-category-hidden", StringComparison.Ordinal));

            var publishedEntry = urls.Single(url =>
                (string?)url.Element(SitemapNamespace + "loc")
                == "https://culinary.example/recipes/published%26safe");
            Assert.Equal(
                "2026-10-04",
                (string?)publishedEntry.Element(SitemapNamespace + "lastmod"));

            var categoryEntry = urls.Single(url =>
                (string?)url.Element(SitemapNamespace + "loc")
                == "https://culinary.example/categories/main-dishes");
            Assert.Equal(
                "2026-10-01",
                (string?)categoryEntry.Element(SitemapNamespace + "lastmod"));

            var staticEntry = urls.Single(url =>
                (string?)url.Element(SitemapNamespace + "loc")
                == "https://culinary.example/");
            Assert.Null(staticEntry.Element(SitemapNamespace + "lastmod"));

            Assert.Contains("&amp;", sitemapText);
            var robots = await File.ReadAllTextAsync(
                Path.Combine(outputDirectory, "robots.txt"));
            Assert.Contains("User-agent: *", robots);
            Assert.Contains("Disallow: /private", robots);
            Assert.Contains("Sitemap: https://culinary.example/sitemap.xml", robots);
            Assert.DoesNotContain("old.example", robots);
            Assert.Equal(1, robots.Split("Sitemap:").Length - 1);
        }
        finally
        {
            if (Directory.Exists(outputDirectory))
            {
                Directory.Delete(outputDirectory, recursive: true);
            }
        }
    }

    [Fact]
    public async Task GenerateAsync_OverwritesExistingSitemapWithoutTemporaryFiles()
    {
        var options = new DbContextOptionsBuilder<CulinaryBlogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var outputDirectory = Path.Combine(
            Path.GetTempPath(),
            $"sitemap-overwrite-tests-{Guid.NewGuid():N}");

        try
        {
            await using var dbContext = new CulinaryBlogDbContext(options);
            Directory.CreateDirectory(outputDirectory);
            var sitemapPath = Path.Combine(outputDirectory, "sitemap.xml");
            await File.WriteAllTextAsync(sitemapPath, "old sitemap");
            var job = new SitemapGenerationJob(
                dbContext,
                outputDirectory,
                "https://culinary.example",
                ["/", "/recipes", "/categories"],
                NullLogger<SitemapGenerationJob>.Instance);

            await job.GenerateAsync(CancellationToken.None);

            var document = XDocument.Load(sitemapPath);
            Assert.Equal(
                [
                    "https://culinary.example/",
                    "https://culinary.example/categories",
                    "https://culinary.example/recipes"
                ],
                document.Root!
                    .Elements(SitemapNamespace + "url")
                    .Select(url => (string)url.Element(SitemapNamespace + "loc")!)
                    .ToArray());
            Assert.Empty(Directory.GetFiles(outputDirectory, "*.tmp"));
        }
        finally
        {
            if (Directory.Exists(outputDirectory))
            {
                Directory.Delete(outputDirectory, recursive: true);
            }
        }
    }

    private static Category CreateCategory(
        string name,
        string slug,
        DateTime createdAt) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Slug = slug,
        CreatedAt = createdAt
    };

    private static Recipe CreateRecipe(
        string slug,
        Category category,
        RecipeStatus status,
        DateTime createdAt,
        DateTime? updatedAt = null,
        bool isDeleted = false) => new()
    {
        Id = Guid.NewGuid(),
        Title = slug,
        Slug = slug,
        Description = "Sitemap test recipe",
        Instructions = "Test only",
        PrepTimeMinutes = 1,
        CookTimeMinutes = 1,
        Servings = 1,
        Difficulty = DifficultyLevel.Easy,
        Status = status,
        IsDeleted = isDeleted,
        CategoryId = category.Id,
        Category = category,
        CreatedAt = createdAt,
        UpdatedAt = updatedAt,
        RowVersion = 1
    };
}

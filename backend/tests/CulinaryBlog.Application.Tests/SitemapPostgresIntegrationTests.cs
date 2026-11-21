using System.Xml.Linq;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Infrastructure.BackgroundJobs;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CulinaryBlog.Application.Tests;

public sealed class SitemapPostgresIntegrationTests
{
    [Fact]
    public async Task IsolatedPostgres_ReflectsPublishUnpublishAndSoftDeletes()
    {
        var connection = Environment.GetEnvironmentVariable("CULINARY_JOBS_TEST_DB");
        if (connection is null) return;
        var options = new DbContextOptionsBuilder<CulinaryBlogDbContext>()
            .UseNpgsql(connection).Options;
        var directory = Path.Combine(Path.GetTempPath(), $"sitemap-pg-{Guid.NewGuid():N}");
        try
        {
            await using var db = new CulinaryBlogDbContext(options);
            await db.Database.MigrateAsync();
            var suffix = Guid.NewGuid().ToString("N");
            var category = Category.Create($"Sitemap PG {suffix}", $"sitemap-{suffix}", null, null, 0);
            var recipe = new Recipe
            {
                Id = Guid.NewGuid(), Title = "Sitemap PG", Slug = $"sitemap-recipe-{suffix}",
                Description = "Test", Instructions = "Test", Category = category,
                PrepTimeMinutes = 1, CookTimeMinutes = 1, Servings = 1,
                Difficulty = DifficultyLevel.Easy, Status = RecipeStatus.Draft,
                CreatedAt = DateTime.UtcNow, RowVersion = 1
            };
            db.Categories.Add(category);
            db.Recipes.Add(recipe);
            await db.SaveChangesAsync();
            var job = new SitemapGenerationJob(db, directory, "https://culinary.test",
                ["/", "/recipes", "/categories"], NullLogger<SitemapGenerationJob>.Instance);

            async Task<string[]> Locations()
            {
                await job.GenerateAsync(CancellationToken.None);
                XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
                var document = XDocument.Load(Path.Combine(directory, "sitemap.xml"));
                Assert.All(document.Root!.Elements(ns + "url"), element =>
                {
                    Assert.NotNull(element.Element(ns + "loc"));
                    Assert.NotNull(element.Element(ns + "changefreq"));
                    Assert.NotNull(element.Element(ns + "priority"));
                });
                var robots = await File.ReadAllTextAsync(Path.Combine(directory, "robots.txt"));
                Assert.Contains("User-agent: *", robots);
                Assert.Contains("Sitemap: https://culinary.test/sitemap.xml", robots);
                return document.Root.Elements(ns + "url")
                    .Select(element => (string)element.Element(ns + "loc")!).ToArray();
            }

            var categoryUrl = $"https://culinary.test/categories/{category.Slug}";
            var recipeUrl = $"https://culinary.test/recipes/{recipe.Slug}";
            Assert.Contains(categoryUrl, await Locations());
            Assert.DoesNotContain(recipeUrl, await Locations());

            recipe.Status = RecipeStatus.Published;
            await db.SaveChangesAsync();
            Assert.Contains(recipeUrl, await Locations());
            XNamespace sitemapNamespace = "http://www.sitemaps.org/schemas/sitemap/0.9";
            var publishedEntry = XDocument.Load(Path.Combine(directory, "sitemap.xml"))
                .Descendants(sitemapNamespace + "url")
                .Single(element => (string)element.Element(sitemapNamespace + "loc")! == recipeUrl);
            Assert.NotNull(publishedEntry.Element(sitemapNamespace + "lastmod"));
            Assert.Equal("0.8", (string?)publishedEntry.Element(sitemapNamespace + "priority"));

            recipe.Status = RecipeStatus.Draft;
            await db.SaveChangesAsync();
            Assert.DoesNotContain(recipeUrl, await Locations());

            recipe.Status = RecipeStatus.Published;
            recipe.IsDeleted = true;
            await db.SaveChangesAsync();
            Assert.DoesNotContain(recipeUrl, await Locations());

            recipe.IsDeleted = false;
            category.IsDeleted = true;
            await db.SaveChangesAsync();
            var afterCategoryDelete = await Locations();
            Assert.DoesNotContain(categoryUrl, afterCategoryDelete);
            Assert.DoesNotContain(recipeUrl, afterCategoryDelete);
            Assert.Contains("https://culinary.test/", afterCategoryDelete);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }
}

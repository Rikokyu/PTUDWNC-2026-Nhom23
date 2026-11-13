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
                return XDocument.Load(Path.Combine(directory, "sitemap.xml"))
                    .Root!.Elements(ns + "url")
                    .Select(element => (string)element.Element(ns + "loc")!).ToArray();
            }

            var categoryUrl = $"https://culinary.test/categories/{category.Slug}";
            var recipeUrl = $"https://culinary.test/recipes/{recipe.Slug}";
            Assert.Contains(categoryUrl, await Locations());
            Assert.DoesNotContain(recipeUrl, await Locations());

            recipe.Status = RecipeStatus.Published;
            await db.SaveChangesAsync();
            Assert.Contains(recipeUrl, await Locations());

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

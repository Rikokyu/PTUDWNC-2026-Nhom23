using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CulinaryBlog.Application.Tests;

public sealed class RecipeSortPostgresTests
{
    [Fact]
    public async Task IsolatedPostgres_AllSixSortsAndStableTieBreaker()
    {
        var connection = Environment.GetEnvironmentVariable("CULINARY_JOBS_TEST_DB");
        if (connection is null) return;
        var options = new DbContextOptionsBuilder<CulinaryBlogDbContext>()
            .UseNpgsql(connection).Options;
        await using var db = new CulinaryBlogDbContext(options);
        await db.Database.MigrateAsync();
        var suffix = Guid.NewGuid().ToString("N");
        var category = Category.Create($"Sort PG {suffix}", $"sort-{suffix}", null, null, 0);
        var baseTime = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);
        var recipes = new[]
        {
            Make("A", 20, baseTime.AddDays(2)),
            Make("C", 10, baseTime),
            Make("B", 10, baseTime.AddDays(1))
        };
        var tieIds = new[] { Guid.NewGuid(), Guid.NewGuid() }.Order().ToArray();
        recipes[2].Id = tieIds[0];
        recipes[1].Id = tieIds[1];
        db.Categories.Add(category);
        db.Recipes.AddRange(recipes);
        await db.SaveChangesAsync();

        var repository = new RecipeRepository(db);
        async Task<string[]> Get(string sort)
        {
            var (items, count) = await repository.GetPagedAsync(
                1, 12, category.Id, null, null, null, sort,
                null, false, false);
            Assert.Equal(3, count);
            return items.Select(recipe => recipe.Title).ToArray();
        }

        Assert.Equal(["A", "B", "C"], await Get("title"));
        Assert.Equal(["C", "B", "A"], await Get("-title"));
        Assert.Equal(["C", "B", "A"], await Get("createdAt"));
        Assert.Equal(["A", "B", "C"], await Get("-createdAt"));
        Assert.Equal(["B", "C", "A"], await Get("cookTime"));
        Assert.Equal(["A", "B", "C"], await Get("-cookTime"));

        recipes[0].Servings = 4;
        recipes[1].Servings = 6;
        recipes[2].Servings = 2;
        await db.SaveChangesAsync();
        var filtered = await repository.GetPagedAsync(1, 1, category.Id,
            DifficultyLevel.Easy, 10, null, "title", null, false, false,
            minServings: 4);
        Assert.Equal(1, filtered.TotalCount);
        Assert.Equal("C", Assert.Single(filtered.Items).Title);
        var beyond = await repository.GetPagedAsync(2, 1, category.Id,
            DifficultyLevel.Easy, 10, null, "title", null, false, false,
            minServings: 4);
        Assert.Equal(1, beyond.TotalCount);
        Assert.Empty(beyond.Items);

        Recipe Make(string title, int cookTime, DateTime createdAt) => new()
        {
            Id = Guid.NewGuid(), Title = title, Slug = $"{title.ToLowerInvariant()}-{suffix}",
            Description = "Test", Instructions = "Test", Category = category,
            PrepTimeMinutes = 1, CookTimeMinutes = cookTime, Servings = 1,
            Difficulty = DifficultyLevel.Easy, Status = RecipeStatus.Published,
            CreatedAt = createdAt, RowVersion = 1
        };
    }
}

using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace CulinaryBlog.Application.Tests;

public class VietnameseSearchPostgresTests
{
    [Theory]
    [InlineData("M\u00cc", "mi:*")]
    [InlineData("mi\u0300", "mi:*")]
    [InlineData("PH\u1ede B\u00d2", "pho:* & bo:*")]
    [InlineData("\u0110\u1eacU", "dau:*")]
    [InlineData("'&|!:*%_", "")]
    public void QueryUsesOnlyLiteralPrefixes(string input, string expected)
        => Assert.Equal(expected, VietnameseSearchQuery.Build(input));

    [PostgresFact]
    public async Task PostgreSql_Migrations_Search_Ranking_Pagination_Triggers_AndRollback()
    {
        // Explicit opt-in: never read the application's connection string.
        var connection = Environment.GetEnvironmentVariable("CULINARY_SEARCH_TEST_DB");
        Assert.False(string.IsNullOrEmpty(connection), "Set CULINARY_SEARCH_TEST_DB to a disposable database.");
        var options = new DbContextOptionsBuilder<CulinaryBlogDbContext>().UseNpgsql(connection).Options;
        await using var db = new CulinaryBlogDbContext(options);
        await db.Database.MigrateAsync();
        var category = new Category { Id = Guid.NewGuid(), Name = "Test", Slug = Guid.NewGuid().ToString() };
        var ownerId = Guid.NewGuid();
        db.ApplicationUsers.Add(new ApplicationUser
        {
            Id = ownerId,
            Email = $"search-{ownerId:N}@example.test",
            UserName = $"search-{ownerId:N}",
            DisplayName = "Search test owner"
        });
        db.Categories.Add(category);
        Recipe Add(string title, string description = "", RecipeStatus status = RecipeStatus.Published, bool deleted = false)
        {
            var r = new Recipe { Id = Guid.NewGuid(), Title = title, Slug = Guid.NewGuid().ToString(), Description = description,
                CategoryId = category.Id, AuthorId = ownerId, Status = status, IsDeleted = deleted, CreatedAt = new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc) };
            db.Recipes.Add(r); return r;
        }
        var mi = Add("M\u00ec b\u00f2"); var pho = Add("Ph\u1edf b\u00f2"); var dau = Add("\u0110\u1eacU");
        var desc = Add("Other", "m\u00ec b\u00f2");
        var instructions = Add("Steps"); instructions.Instructions = "c\u00e1 kho";
        var ingredient = Add("Ingredient"); ingredient.Ingredients.Add(new RecipeIngredient { Id=Guid.NewGuid(), Name="t\u1ecfi \u0111en" });
        Add("M\u00ec b\u00f2", status: RecipeStatus.Draft); Add("M\u00ec b\u00f2", status: RecipeStatus.Archived); Add("M\u00ec b\u00f2", deleted:true);
        await db.SaveChangesAsync();
        var repo = new RecipeRepository(db);
        foreach (var q in new[] { "mi", "m\u00ec", "M\u00cc", "mi\u0300" })
        {
            var result = await repo.SearchPublishedAsync(q,1,50);
            Assert.Equal(2, result.TotalCount); Assert.Equal(mi.Id,result.Items[0].Id);
            Assert.True(result.Items[0].SearchRelevanceScore > result.Items[1].SearchRelevanceScore);
        }
        foreach(var q in new[] { "pho b", "ph\u1edf b\u00f2", "PHO BO" }) Assert.Equal(pho.Id, Assert.Single((await repo.SearchPublishedAsync(q,1,50)).Items).Id);
        foreach (var q in new[] { "pho bo", "Ph\u1edf b\u00f2" })
        {
            var publicListResult = await repo.GetPagedAsync(1, 50, null, null, null, q,
                "-createdAt", null, false, false);
            Assert.Equal(pho.Id, Assert.Single(publicListResult.Items).Id);
            var authorListResult = await repo.GetPagedAsync(1, 50, null, null, null, q,
                "-createdAt", ownerId, true, false, ownRecipesOnly: true);
            Assert.Equal(pho.Id, Assert.Single(authorListResult.Items).Id);
            var otherAuthorListResult = await repo.GetPagedAsync(1, 50, null, null, null, q,
                "-createdAt", Guid.NewGuid(), true, false, ownRecipesOnly: true);
            Assert.Empty(otherAuthorListResult.Items);
            var adminResult = await repo.GetPagedAsync(1, 50, null, null, null, q,
                "-createdAt", null, true, true, includeAllStatuses: true);
            Assert.Equal(pho.Id, Assert.Single(adminResult.Items).Id);
        }
        var allStatuses = await repo.GetPagedAsync(1, 50, null, null, null,
            "mi bo", "-createdAt", null, true, true, includeAllStatuses: true);
        Assert.Equal(4, allStatuses.TotalCount);
        Assert.Contains(allStatuses.Items, item => item.Status == RecipeStatus.Draft);
        Assert.Contains(allStatuses.Items, item => item.Status == RecipeStatus.Archived);
        foreach(var q in new[] { "dau", "\u0110\u1eacU", "\u0111\u1eadu" }) Assert.Equal(dau.Id, Assert.Single((await repo.SearchPublishedAsync(q,1,50)).Items).Id);
        Assert.Equal(instructions.Id, Assert.Single((await repo.SearchPublishedAsync("ca kho",1,50)).Items).Id);
        Assert.Equal(ingredient.Id, Assert.Single((await repo.SearchPublishedAsync("toi d",1,50)).Items).Id);
        var first = await repo.SearchPublishedAsync("mi",1,1); var second = await repo.SearchPublishedAsync("mi",2,1);
        Assert.Equal(2,first.TotalCount); Assert.NotEqual(first.Items[0].Id,second.Items[0].Id);
        Assert.Empty((await repo.SearchPublishedAsync("'&|!:*%_",1,50)).Items);
        Assert.Empty((await repo.SearchPublishedAsync("zznotfound",1,50)).Items);
        Assert.Empty((await repo.SearchPublishedAsync("'; DROP TABLE Recipes;--",1,50)).Items);
        mi.Servings = 4; mi.CookTimeMinutes = 10; mi.Difficulty = DifficultyLevel.Easy;
        desc.Servings = 2; desc.CookTimeMinutes = 30; desc.Difficulty = DifficultyLevel.Hard;
        await db.SaveChangesAsync();
        var filtered = await repo.SearchPublishedAsync("mi", 1, 1,
            categoryId: category.Id, difficulty: DifficultyLevel.Easy,
            maxCookTime: 10, minServings: 4);
        Assert.Equal(1, filtered.TotalCount);
        Assert.Equal(mi.Id, Assert.Single(filtered.Items).Id);
        Assert.Empty((await repo.SearchPublishedAsync("mi", 2, 1,
            minServings: 4)).Items);
        Assert.Empty((await repo.SearchPublishedAsync("mi", 1, 10,
            categoryId: Guid.NewGuid())).Items);
        mi.Title = "B\u00fan"; await db.SaveChangesAsync();
        Assert.Equal(1,(await repo.SearchPublishedAsync("mi",1,50)).TotalCount);
        ingredient.Ingredients.Single().Name="g\u1eebng"; await db.SaveChangesAsync();
        Assert.Empty((await repo.SearchPublishedAsync("toi",1,50)).Items);
        Assert.Single((await repo.SearchPublishedAsync("gung",1,50)).Items);
        category.IsDeleted=true; await db.SaveChangesAsync();
        Assert.Empty((await repo.SearchPublishedAsync("pho",1,50)).Items);
        var migrator = db.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrator>();
        await migrator.MigrateAsync("20261107090000_AddSavedRecipes");
        await migrator.MigrateAsync();
    }
}

public sealed class PostgresFactAttribute : FactAttribute
{
    public PostgresFactAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CULINARY_SEARCH_TEST_DB")))
            Skip = "Requires an explicitly configured disposable PostgreSQL database.";
    }
}

using CulinaryBlog.Application.Features.Recipes.Queries.GetAuthorProfile;
using CulinaryBlog.Application.Features.SavedRecipes.Queries.GetRecipeSaveCount;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CulinaryBlog.Application.Tests;

public sealed class AuthorProfileAndSaveCountPostgresTests
{
    [Fact]
    public async Task PublicProfileAndSaveCount_ExposeOnlyPublishedVisibleRecipes()
    {
        var connection = Environment.GetEnvironmentVariable("CULINARY_JOBS_TEST_DB");
        if (string.IsNullOrWhiteSpace(connection)) return;

        var options = new DbContextOptionsBuilder<CulinaryBlogDbContext>()
            .UseNpgsql(connection).Options;
        await using var db = new CulinaryBlogDbContext(options);
        await using var transaction = await db.Database.BeginTransactionAsync();
        var suffix = Guid.NewGuid().ToString("N");
        var author = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = $"author-{suffix}",
            Email = $"author-{suffix}@example.test",
            DisplayName = "Test author",
            Bio = "Public biography",
            AvatarUrl = "/uploads/avatar.png",
            CreatedAt = DateTime.UtcNow
        };
        var saver = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = $"saver-{suffix}",
            Email = $"saver-{suffix}@example.test",
            DisplayName = "Test saver",
            CreatedAt = DateTime.UtcNow
        };
        var category = Category.Create($"Author test {suffix}", $"author-test-{suffix}", null, null, 0);
        db.ApplicationUsers.AddRange(author, saver);
        db.Categories.Add(category);

        Recipe MakeRecipe(string name, RecipeStatus status, bool isDeleted = false) => new()
        {
            Id = Guid.NewGuid(),
            Title = name,
            Slug = $"{name}-{suffix}",
            Description = name,
            Instructions = name,
            Category = category,
            Author = author,
            Status = status,
            IsDeleted = isDeleted,
            CreatedAt = DateTime.UtcNow
        };

        var published = MakeRecipe("published", RecipeStatus.Published);
        var draft = MakeRecipe("draft", RecipeStatus.Draft);
        var archived = MakeRecipe("archived", RecipeStatus.Archived);
        var deleted = MakeRecipe("deleted", RecipeStatus.Published, true);
        var unattributed = MakeRecipe("editorial", RecipeStatus.Published);
        unattributed.Author = null;
        unattributed.AuthorId = null;
        db.Recipes.AddRange(published, draft, archived, deleted, unattributed);
        await db.SaveChangesAsync();
        db.SavedRecipes.AddRange(
            new SavedRecipe { UserId = saver.Id, RecipeId = published.Id, CreatedAt = DateTime.UtcNow },
            new SavedRecipe { UserId = saver.Id, RecipeId = draft.Id, CreatedAt = DateTime.UtcNow },
            new SavedRecipe { UserId = saver.Id, RecipeId = deleted.Id, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var recipes = new RecipeRepository(db);
        var savedRecipes = new SavedRecipeRepository(db);
        var profile = await new GetAuthorProfileQueryHandler(recipes)
            .Handle(new GetAuthorProfileQuery(author.Id, 1, 12), CancellationToken.None);
        Assert.Equal("Test author", profile.DisplayName);
        Assert.Equal("Public biography", profile.Bio);
        Assert.Equal(1, profile.Recipes.TotalCount);
        var item = Assert.Single(profile.Recipes.Items);
        Assert.Equal(published.Id, item.Id);
        Assert.Equal(author.Id, item.Author?.Id);
        Assert.Equal(author.AvatarUrl, item.Author?.AvatarUrl);

        var editorialProfile = await new GetAuthorProfileQueryHandler(recipes)
            .Handle(new GetAuthorProfileQuery(null, 1, 50), CancellationToken.None);
        Assert.Equal("Culinary Blog", editorialProfile.DisplayName);
        Assert.Contains(editorialProfile.Recipes.Items, recipe => recipe.Id == unattributed.Id);
        Assert.DoesNotContain(editorialProfile.Recipes.Items, recipe => recipe.Id == draft.Id || recipe.Id == archived.Id || recipe.Id == deleted.Id);

        var countHandler = new GetRecipeSaveCountQueryHandler(savedRecipes);
        Assert.Equal(1, await countHandler.Handle(
            new GetRecipeSaveCountQuery(published.Id), CancellationToken.None));
        await Assert.ThrowsAsync<CulinaryBlog.Application.Common.Exceptions.NotFoundException>(
            () => countHandler.Handle(new GetRecipeSaveCountQuery(draft.Id), CancellationToken.None));

        await transaction.RollbackAsync();
    }
}

using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Infrastructure.Persistence.Repositories;
using Xunit;

namespace CulinaryBlog.Application.Tests;

public sealed class CategoryRecipeVisibilityTests
{
    [Fact]
    public void Guest_CanOnlySeePublishedRecipes()
    {
        var published = CreateRecipe(RecipeStatus.Published, Guid.NewGuid());
        var draft = CreateRecipe(RecipeStatus.Draft, Guid.NewGuid());
        var archived = CreateRecipe(RecipeStatus.Archived, Guid.NewGuid());

        AssertVisibleStatuses(
            currentUserId: null,
            includeOwnDrafts: false,
            isAdmin: false,
            published,
            draft,
            archived,
            RecipeStatus.Published);
    }

    [Fact]
    public void Admin_CanSeeRecipesInEveryStatusRegardlessOfAuthor()
    {
        var adminId = Guid.NewGuid();
        var published = CreateRecipe(RecipeStatus.Published, Guid.NewGuid());
        var adminDraft = CreateRecipe(RecipeStatus.Draft, adminId);
        var adminArchived = CreateRecipe(RecipeStatus.Archived, adminId);
        var anotherAuthorsDraft = CreateRecipe(RecipeStatus.Draft, Guid.NewGuid());
        var anotherAuthorsArchived = CreateRecipe(RecipeStatus.Archived, Guid.NewGuid());

        AssertVisibleStatuses(
            currentUserId: adminId,
            includeOwnDrafts: false,
            isAdmin: true,
            published,
            adminDraft,
            adminArchived,
            RecipeStatus.Published,
            RecipeStatus.Draft,
            RecipeStatus.Archived);

        var predicate = RecipeRepository.CreateCategoryVisibilityPredicate(adminId, false, true).Compile();
        Assert.True(predicate(anotherAuthorsDraft));
        Assert.True(predicate(anotherAuthorsArchived));
    }

    [Fact]
    public void Author_CanSeePublishedAndOwnPrivateRecipes()
    {
        var authorId = Guid.NewGuid();
        var published = CreateRecipe(RecipeStatus.Published, Guid.NewGuid());
        var ownDraft = CreateRecipe(RecipeStatus.Draft, authorId);
        var anotherAuthorsDraft = CreateRecipe(RecipeStatus.Draft, Guid.NewGuid());
        var ownArchived = CreateRecipe(RecipeStatus.Archived, authorId);

        AssertVisibleStatuses(
            authorId,
            includeOwnDrafts: true,
            isAdmin: false,
            published,
            ownDraft,
            anotherAuthorsDraft,
            ownArchived,
            RecipeStatus.Published,
            RecipeStatus.Draft,
            RecipeStatus.Archived);
    }

    private static void AssertVisibleStatuses(
        Guid? currentUserId,
        bool includeOwnDrafts,
        bool isAdmin,
        Recipe published,
        Recipe draft,
        Recipe archived,
        params RecipeStatus[] expectedStatuses)
    {
        var predicate = RecipeRepository
            .CreateCategoryVisibilityPredicate(
                currentUserId,
                includeOwnDrafts,
                isAdmin)
            .Compile();

        var visible = new[] { published, draft, archived }
            .Where(predicate)
            .Select(recipe => recipe.Status)
            .ToArray();

        Assert.Equal(expectedStatuses, visible);
    }

    private static void AssertVisibleStatuses(
        Guid? currentUserId,
        bool includeOwnDrafts,
        bool isAdmin,
        Recipe published,
        Recipe ownDraft,
        Recipe anotherAuthorsDraft,
        Recipe ownArchived,
        params RecipeStatus[] expectedStatuses)
    {
        var predicate = RecipeRepository
            .CreateCategoryVisibilityPredicate(
                currentUserId,
                includeOwnDrafts,
                isAdmin)
            .Compile();

        var visible = new[]
            {
                published,
                ownDraft,
                anotherAuthorsDraft,
                ownArchived
            }
            .Where(predicate)
            .Select(recipe => recipe.Status)
            .ToArray();

        Assert.Equal(expectedStatuses, visible);
    }

    private static Recipe CreateRecipe(
        RecipeStatus status,
        Guid authorId) =>
        new()
        {
            Status = status,
            AuthorId = authorId
        };
}

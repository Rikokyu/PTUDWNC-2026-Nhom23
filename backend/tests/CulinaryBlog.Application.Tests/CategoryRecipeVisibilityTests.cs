using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Infrastructure.Persistence.Repositories;
using Xunit;

namespace CulinaryBlog.Application.Tests;

public sealed class CategoryRecipeVisibilityTests
{
    [Fact]
    public void GuestAndAdmin_CanOnlySeePublishedRecipes()
    {
        var adminId = Guid.NewGuid();
        var published = CreateRecipe(RecipeStatus.Published, Guid.NewGuid());
        var adminDraft = CreateRecipe(RecipeStatus.Draft, adminId);
        var adminArchived = CreateRecipe(RecipeStatus.Archived, adminId);

        AssertVisibleStatuses(
            currentUserId: adminId,
            includeOwnDrafts: false,
            published,
            adminDraft,
            adminArchived,
            RecipeStatus.Published);
    }

    [Fact]
    public void Author_CanSeePublishedAndOwnDrafts_ButNotOthersDraftsOrArchived()
    {
        var authorId = Guid.NewGuid();
        var published = CreateRecipe(RecipeStatus.Published, Guid.NewGuid());
        var ownDraft = CreateRecipe(RecipeStatus.Draft, authorId);
        var anotherAuthorsDraft = CreateRecipe(RecipeStatus.Draft, Guid.NewGuid());
        var ownArchived = CreateRecipe(RecipeStatus.Archived, authorId);

        AssertVisibleStatuses(
            authorId,
            includeOwnDrafts: true,
            published,
            ownDraft,
            anotherAuthorsDraft,
            ownArchived,
            RecipeStatus.Published,
            RecipeStatus.Draft);
    }

    private static void AssertVisibleStatuses(
        Guid? currentUserId,
        bool includeOwnDrafts,
        Recipe published,
        Recipe draft,
        Recipe archived,
        params RecipeStatus[] expectedStatuses)
    {
        var predicate = RecipeRepository
            .CreateCategoryVisibilityPredicate(
                currentUserId,
                includeOwnDrafts)
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
        Recipe published,
        Recipe ownDraft,
        Recipe anotherAuthorsDraft,
        Recipe ownArchived,
        params RecipeStatus[] expectedStatuses)
    {
        var predicate = RecipeRepository
            .CreateCategoryVisibilityPredicate(
                currentUserId,
                includeOwnDrafts)
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

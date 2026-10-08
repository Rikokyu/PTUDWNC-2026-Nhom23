using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Helpers;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Domain.Interfaces;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.DTOs.Recipes;

namespace CulinaryBlog.Application.Features.Recipes;

internal static class RecipeCommandHelper
{
    public static void EnsureValidFields(
        string? title,
        string? description,
        string? instructions,
        int prepTimeMinutes,
        int cookTimeMinutes,
        int servings,
        DifficultyLevel difficulty)
    {
        if (string.IsNullOrWhiteSpace(title)
            || title.Trim().Length > 200)
        {
            throw new ValidationException(
                "Title is required and must not exceed 200 characters.");
        }

        if (string.IsNullOrWhiteSpace(description)
            || string.IsNullOrWhiteSpace(instructions))
        {
            throw new ValidationException(
                "Description and instructions are required.");
        }

        if (prepTimeMinutes < 0
            || cookTimeMinutes < 0
            || servings < 1
            || !Enum.IsDefined(difficulty))
        {
            throw new ValidationException(
                "Recipe times must be non-negative, servings must be positive, and difficulty must be valid.");
        }

        if (string.IsNullOrWhiteSpace(SlugHelper.Generate(title)))
        {
            throw new ValidationException(
                "Title must contain at least one letter or number.");
        }
    }

    public static async Task<string> CreateUniqueSlugAsync(
        IRecipeRepository recipes,
        string title,
        Guid? excludingId,
        CancellationToken cancellationToken)
    {
        var baseSlug = SlugHelper.Generate(title);
        var slug = baseSlug;
        var suffix = 2;

        while (await recipes.SlugExistsAsync(
                   slug,
                   excludingId,
                   cancellationToken))
        {
            slug = $"{baseSlug}-{suffix++}";
        }

        return slug;
    }

    public static void EnsureCanManage(
        ICurrentUser currentUser,
        Recipe recipe)
    {
        if (!currentUser.IsAuthenticated)
        {
            throw new UnauthorizedException(
                "Authentication is required.");
        }

        if (!currentUser.IsAdmin
            && (!currentUser.UserId.HasValue
                || recipe.AuthorId != currentUser.UserId))
        {
            throw new ForbiddenException(
                "You do not have permission to modify this recipe.");
        }
    }

    public static void EnsureAuthenticatedAuthor(ICurrentUser currentUser)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            throw new UnauthorizedException(
                "An authenticated user is required to create recipes.");
        }
    }

    public static RecipeSummaryDto ToSummary(
        Recipe recipe,
        Category category) =>
        new(
            recipe.Id,
            recipe.Title,
            recipe.Slug,
            recipe.Images
                .OrderByDescending(image => image.IsPrimary)
                .ThenBy(image => image.OrderIndex)
                .Select(image =>
                    image.ThumbnailUrl
                    ?? image.MediumUrl
                    ?? image.OriginalUrl)
                .FirstOrDefault(),
            category.Name,
            recipe.CategoryId,
            recipe.PrepTimeMinutes,
            recipe.CookTimeMinutes,
            recipe.Servings,
            recipe.Difficulty,
            recipe.Status,
            recipe.CreatedAt);
}

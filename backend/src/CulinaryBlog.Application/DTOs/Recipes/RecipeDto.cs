using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Domain.Entities;

namespace CulinaryBlog.Application.DTOs.Recipes;

public sealed record RecipeSummaryDto(
    Guid Id,
    string Title,
    string Slug,
    string? ThumbnailUrl,
    string CategoryName,
    Guid CategoryId,
    int PrepTimeMinutes,
    int CookTimeMinutes,
    int Servings,
    DifficultyLevel Difficulty,
    RecipeStatus Status,
    DateTime CreatedAt,
    float? RelevanceScore = null,
    RecipeCardAuthorDto? Author = null
)
{
    public static RecipeSummaryDto FromRecipe(Recipe recipe, float? relevanceScore = null) =>
        new(
            recipe.Id,
            recipe.Title,
            recipe.Slug,
            recipe.Images.OrderByDescending(image => image.IsPrimary)
                .ThenBy(image => image.OrderIndex)
                .Select(image => image.ThumbnailUrl ?? image.MediumUrl ?? image.OriginalUrl)
                .FirstOrDefault(),
            recipe.Category.Name,
            recipe.CategoryId,
            recipe.PrepTimeMinutes,
            recipe.CookTimeMinutes,
            recipe.Servings,
            recipe.Difficulty,
            recipe.Status,
            recipe.CreatedAt,
            relevanceScore,
            recipe.Author is null ? null : new RecipeCardAuthorDto(
                recipe.Author.Id,
                recipe.Author.DisplayName,
                recipe.Author.AvatarUrl));
}

public sealed record RecipeCardAuthorDto(
    Guid Id,
    string DisplayName,
    string? AvatarUrl);

public sealed record RecipeDetailDto(
    Guid Id,
    string Title,
    string Slug,
    string Description,
    string Instructions,
    int PrepTimeMinutes,
    int CookTimeMinutes,
    int Servings,
    DifficultyLevel Difficulty,
    RecipeStatus Status,
    DateTime CreatedAt,
    uint RowVersion,
    RecipeCategoryDto Category,
    RecipeAuthorDto? Author,
    IReadOnlyList<RecipeIngredientDto> Ingredients,
    IReadOnlyList<RecipeStepDto> Steps,
    IReadOnlyList<RecipeImageDto> Images,
    RecipeNutritionDto? Nutrition
);

public sealed record RecipeCategoryDto(
    Guid Id,
    string Name,
    string Slug
);

public sealed record RecipeAuthorDto(
    Guid Id,
    string DisplayName,
    string Email,
    string? AvatarUrl = null,
    string? Bio = null
);

public sealed record RecipeIngredientDto(
    Guid Id,
    string Name,
    string? Quantity,
    string? Unit,
    string? Notes,
    int OrderIndex
);

public sealed record RecipeStepDto(
    Guid Id,
    int StepNumber,
    string Title,
    string Description,
    int? TimerMinutes,
    string? ImageUrl
);

public sealed record RecipeImageDto(
    Guid Id,
    string OriginalUrl,
    string? MediumUrl,
    string? ThumbnailUrl,
    string? AltText,
    bool IsPrimary,
    int OrderIndex
);

public sealed record RecipeNutritionDto(
    decimal? Calories,
    decimal? Protein,
    decimal? Carbs,
    decimal? Fat,
    decimal? Fiber,
    decimal? Sodium
);

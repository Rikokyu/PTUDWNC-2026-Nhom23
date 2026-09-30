using CulinaryBlog.Domain.Enums;

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
    DateTime CreatedAt
);

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
    string Email
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
namespace CulinaryBlog.Application.DTOs.Recipes;

public sealed record RecipeSummaryDto(
    Guid Id,
    string Title,
    string Slug,
    string Summary,
    string Description,
    string? PrimaryImageUrl,
    int PrepTimeMinutes,
    int CookTimeMinutes,
    int Servings,
    string Difficulty,
    string CategoryName,
    string CategorySlug,
    string AuthorName,
    string? AuthorAvatar,
    string AuthorRole,
    DateTime CreatedAt,
    double Rating,
    int TotalReviews,
    bool IsFeatured,
    int LikeCount,
    bool IsLiked);

public sealed record RecipeDetailDto(
    Guid Id,
    string Title,
    string Slug,
    string Summary,
    string Description,
    string? PrimaryImageUrl,
    int PrepTimeMinutes,
    int CookTimeMinutes,
    int Servings,
    string Difficulty,
    string Status,
    string CategoryName,
    string CategorySlug,
    string AuthorName,
    string? AuthorAvatar,
    string AuthorRole,
    DateTime CreatedAt,
    double Rating,
    int TotalReviews,
    bool IsFeatured,
    int LikeCount,
    bool IsLiked,
    IReadOnlyList<RecipeIngredientDto> Ingredients,
    IReadOnlyList<RecipeStepDto> Steps,
    IReadOnlyList<RecipeImageDto> Images);

public sealed record RecipeIngredientDto(
    Guid Id,
    string Name,
    decimal? Quantity,
    string? Unit,
    string? Notes,
    int OrderIndex);

public sealed record RecipeStepDto(
    Guid Id,
    int StepNumber,
    string? Title,
    string Description,
    int? TimerMinutes,
    string? ImageUrl);

public sealed record RecipeImageDto(
    Guid Id,
    string OriginalUrl,
    string? MediumUrl,
    string? ThumbnailUrl,
    string? AltText,
    bool IsPrimary,
    int OrderIndex);

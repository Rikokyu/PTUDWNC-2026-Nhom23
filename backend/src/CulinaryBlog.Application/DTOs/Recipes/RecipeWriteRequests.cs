using CulinaryBlog.Domain.Enums;

namespace CulinaryBlog.Application.DTOs.Recipes;

public sealed record RecipeNutritionInput(
    decimal? Calories,
    decimal? Protein,
    decimal? Carbs,
    decimal? Fat,
    decimal? Fiber,
    decimal? Sodium);

public sealed record RecipeIngredientInput(
    string Name,
    string? Quantity,
    string? Unit,
    string? Notes,
    int? OrderIndex = null);

public sealed record RecipeStepInput(
    string Title,
    string Description,
    int? TimerMinutes,
    string? ImageUrl);

public sealed record RecipeImageMetadataInput(
    string? AltText,
    bool? IsPrimary,
    int? OrderIndex);

public sealed record CreateRecipeRequest(
    string Title,
    string Description,
    Guid CategoryId,
    int PrepTimeMinutes,
    int CookTimeMinutes,
    int Servings,
    DifficultyLevel Difficulty,
    RecipeNutritionInput? Nutrition,
    IReadOnlyList<RecipeIngredientInput>? Ingredients,
    IReadOnlyList<RecipeStepInput>? Steps);

public sealed record UpdateRecipeRequest(
    string? Title,
    string? Description,
    Guid? CategoryId,
    int? PrepTimeMinutes,
    int? CookTimeMinutes,
    int? Servings,
    DifficultyLevel? Difficulty,
    RecipeNutritionInput? Nutrition);
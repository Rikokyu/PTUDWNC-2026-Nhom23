using CulinaryBlog.Application.DTOs.Recipes;
using CulinaryBlog.Domain.Enums;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes.Commands;

public sealed record RecipeMutationResult(Guid Id, string Slug);

public sealed record RecipeIngredientInput(
    string Name,
    string? Quantity,
    string? Unit,
    string? Notes);

public sealed record RecipeStepInput(
    string Title,
    string Description,
    int? TimerMinutes,
    string? ImageUrl);

public sealed record CreateRecipeCommand(
    string Title,
    string Description,
    string? Instructions,
    Guid CategoryId,
    int PrepTimeMinutes,
    int CookTimeMinutes,
    int Servings,
    DifficultyLevel Difficulty,
    RecipeNutritionDto? Nutrition,
    IReadOnlyList<RecipeIngredientInput> Ingredients,
    IReadOnlyList<RecipeStepInput> Steps)
    : IRequest<RecipeMutationResult>;

public sealed record UpdateRecipeCommand(
    Guid RecipeId,
    string Title,
    string Description,
    string? Instructions,
    Guid CategoryId,
    int PrepTimeMinutes,
    int CookTimeMinutes,
    int Servings,
    DifficultyLevel Difficulty,
    RecipeNutritionDto? Nutrition)
    : IRequest;

public sealed record SetRecipeStatusCommand(
    Guid RecipeId,
    RecipeStatus Status)
    : IRequest;

public sealed record DeleteRecipeCommand(Guid RecipeId) : IRequest;

public sealed record AddRecipeIngredientCommand(
    Guid RecipeId,
    string Name,
    string? Quantity,
    string? Unit,
    string? Notes)
    : IRequest<Guid>;

public sealed record UpdateRecipeIngredientCommand(
    Guid RecipeId,
    Guid IngredientId,
    string Name,
    string? Quantity,
    string? Unit,
    string? Notes)
    : IRequest;

public sealed record DeleteRecipeIngredientCommand(
    Guid RecipeId,
    Guid IngredientId)
    : IRequest;

public sealed record AddRecipeStepCommand(
    Guid RecipeId,
    string Title,
    string Description,
    int? TimerMinutes,
    string? ImageUrl)
    : IRequest<Guid>;

public sealed record UpdateRecipeStepCommand(
    Guid RecipeId,
    Guid StepId,
    string Title,
    string Description,
    int? TimerMinutes,
    string? ImageUrl)
    : IRequest;

public sealed record DeleteRecipeStepCommand(Guid RecipeId, Guid StepId)
    : IRequest;

public sealed record AddRecipeImageCommand(
    Guid RecipeId,
    string OriginalUrl,
    string? AltText,
    bool IsPrimary)
    : IRequest<Guid>;

public sealed record UpdateRecipeImageCommand(
    Guid RecipeId,
    Guid ImageId,
    string? AltText,
    bool? IsPrimary)
    : IRequest;

public sealed record DeleteRecipeImageCommand(Guid RecipeId, Guid ImageId)
    : IRequest;
using CulinaryBlog.Application.DTOs.Recipes;
using CulinaryBlog.Domain.Enums;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes.Commands.UpdateRecipe;

public sealed record UpdateRecipeCommand(
    Guid Id,
    string? Title,
    string? Description,
    string? Instructions,
    Guid CategoryId,
    int PrepTimeMinutes,
    int CookTimeMinutes,
    int Servings,
    DifficultyLevel Difficulty) : IRequest<RecipeSummaryDto>;
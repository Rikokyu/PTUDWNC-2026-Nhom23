using CulinaryBlog.Application.DTOs.Recipes;
using CulinaryBlog.Domain.Enums;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes.Commands.CreateRecipe;

public sealed record CreateRecipeCommand(
    string? Title,
    string? Description,
    string? Instructions,
    Guid CategoryId,
    int PrepTimeMinutes,
    int CookTimeMinutes,
    int Servings,
    DifficultyLevel Difficulty) : IRequest<RecipeSummaryDto>;
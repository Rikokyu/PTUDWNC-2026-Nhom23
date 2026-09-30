using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.DTOs.Recipes;
using CulinaryBlog.Domain.Enums;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes.Queries.GetRecipes;

public sealed record GetRecipesQuery(
    int Page,
    int PageSize,
    Guid? CategoryId,
    DifficultyLevel? Difficulty,
    int? MaxCookTime,
    string Sort
) : IRequest<PagedResult<RecipeSummaryDto>>;
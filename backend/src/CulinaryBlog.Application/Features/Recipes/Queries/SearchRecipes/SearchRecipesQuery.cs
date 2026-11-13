using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.DTOs.Recipes;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes.Queries.SearchRecipes;

public sealed record SearchRecipesQuery(
    string SearchTerm,
    int Page,
    int PageSize,
    Guid? CategoryId = null,
    CulinaryBlog.Domain.Enums.DifficultyLevel? Difficulty = null,
    int? MaxCookTime = null,
    int? MinServings = null) : IRequest<PagedResult<RecipeSummaryDto>>;
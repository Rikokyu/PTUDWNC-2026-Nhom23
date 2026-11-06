using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.DTOs.Recipes;
using MediatR;

namespace CulinaryBlog.Application.Features.SavedRecipes.Queries.GetSavedRecipes;

public sealed record GetSavedRecipesQuery(int Page, int PageSize)
    : IRequest<PagedResult<RecipeSummaryDto>>;

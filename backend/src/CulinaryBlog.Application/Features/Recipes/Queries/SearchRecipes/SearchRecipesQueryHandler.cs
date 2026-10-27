using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.DTOs.Recipes;
using CulinaryBlog.Domain.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes.Queries.SearchRecipes;

public sealed class SearchRecipesQueryHandler(
    IRecipeRepository recipes)
    : IRequestHandler<SearchRecipesQuery, PagedResult<RecipeSummaryDto>>
{
    public async Task<PagedResult<RecipeSummaryDto>> Handle(
        SearchRecipesQuery request,
        CancellationToken cancellationToken)
    {
        var term = request.SearchTerm?.Trim() ?? string.Empty;
        if (term.Length < 2 || term.Length > 100)
        {
            throw new ValidationException(
                "Search term must contain between 2 and 100 characters.");
        }

        if (request.Page < 1 || request.PageSize is < 1 or > 50)
        {
            throw new ValidationException(
                "Page must be positive and pageSize must be between 1 and 50.");
        }

        var result = await recipes.SearchPublishedAsync(
            term,
            request.Page,
            request.PageSize,
            cancellationToken);
        var items = result.Items
            .Select(recipe => new RecipeSummaryDto(
                recipe.Id,
                recipe.Title,
                recipe.Slug,
                recipe.Images
                    .OrderByDescending(image => image.IsPrimary)
                    .ThenBy(image => image.OrderIndex)
                    .Select(image => image.ThumbnailUrl
                        ?? image.MediumUrl
                        ?? image.OriginalUrl)
                    .FirstOrDefault(),
                recipe.Category.Name,
                recipe.CategoryId,
                recipe.PrepTimeMinutes,
                recipe.CookTimeMinutes,
                recipe.Servings,
                recipe.Difficulty,
                recipe.Status,
                recipe.CreatedAt))
            .ToList();

        return new PagedResult<RecipeSummaryDto>(
            items,
            result.TotalCount,
            request.Page,
            request.PageSize);
    }
}
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.DTOs.Recipes;
using CulinaryBlog.Domain.Interfaces;
using CulinaryBlog.Application.Common.Helpers;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes.Queries.SearchRecipes;

public sealed class SearchRecipesQueryHandler
    : IRequestHandler<SearchRecipesQuery, PagedResult<RecipeSummaryDto>>
{
    private readonly IRecipeRepository _recipeRepository;

    public SearchRecipesQueryHandler(IRecipeRepository recipeRepository)
    {
        _recipeRepository = recipeRepository;
    }

    public async Task<PagedResult<RecipeSummaryDto>> Handle(
        SearchRecipesQuery request,
        CancellationToken cancellationToken)
    {
        var searchTerm = RecipeSearchText.Normalize(request.SearchTerm ?? string.Empty);
        if (searchTerm.Length < 2 || searchTerm.Length > RecipeSearchText.MaxLength
            || string.IsNullOrEmpty(RecipeSearchText.BuildPrefixQuery(searchTerm)))
        {
            throw new ValidationException("q must contain between 2 and 200 characters and at least one letter or number.");
        }

        if (request.Page < 1 || request.PageSize is < 1 or > 50
            || (long)(request.Page - 1) * request.PageSize > int.MaxValue)
        {
            throw new ValidationException("page must be positive and pageSize must be between 1 and 50.");
        }

        var result = await _recipeRepository.SearchAsync(
            searchTerm,
            request.Page,
            request.PageSize,
            SearchRecipesQuery.NormalizeSort(request.Sort),
            cancellationToken);

        var items = result.Items
            .Select(match =>
            {
                var recipe = match.Recipe;
                return new RecipeSummaryDto(
                recipe.Id,
                recipe.Title,
                recipe.Slug,
                recipe.Images
                    .OrderByDescending(image => image.IsPrimary)
                    .ThenBy(image => image.OrderIndex)
                    .Select(image => image.ThumbnailUrl ?? image.MediumUrl ?? image.OriginalUrl)
                    .FirstOrDefault(),
                recipe.Category.Name,
                recipe.CategoryId,
                recipe.PrepTimeMinutes,
                recipe.CookTimeMinutes,
                recipe.Servings,
                recipe.Difficulty,
                recipe.Status,
                recipe.CreatedAt)
                { RelevanceScore = match.RelevanceScore };
            })
            .ToList();

        return new PagedResult<RecipeSummaryDto>(
            items,
            result.TotalCount,
            request.Page,
            request.PageSize)
        {
            Message = result.TotalCount == 0
                ? "Không tìm thấy công thức phù hợp. Hãy thử từ khóa khác hoặc ít từ khóa hơn."
                : null
        };
    }
}

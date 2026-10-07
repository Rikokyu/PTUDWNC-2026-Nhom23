using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.DTOs.Recipes;
using CulinaryBlog.Domain.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes.Queries.GetRecipes;

public sealed class GetRecipesQueryHandler
    : IRequestHandler<
        GetRecipesQuery,
        PagedResult<RecipeSummaryDto>>
{
    private readonly IRecipeRepository _recipeRepository;
    private readonly ICurrentUser _currentUser;

    public GetRecipesQueryHandler(
        IRecipeRepository recipeRepository,
        ICurrentUser currentUser)
    {
        _recipeRepository = recipeRepository;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<RecipeSummaryDto>> Handle(
        GetRecipesQuery request,
        CancellationToken cancellationToken)
    {
        var result =
            await _recipeRepository.GetPagedAsync(
                request.Page,
                request.PageSize,
                request.CategoryId,
                request.Difficulty,
                request.MaxCookTime,
                request.Sort,
                _currentUser.UserId,
                _currentUser.IsAuthenticated,
                _currentUser.IsAdmin,
                request.OwnRecipesOnly,
                request.IncludeAllStatuses,
                cancellationToken);

        var items =
            result.Items
                .Select(recipe =>
                    new RecipeSummaryDto(
                        recipe.Id,
                        recipe.Title,
                        recipe.Slug,
                        recipe.Images
                            .OrderByDescending(x =>
                                x.IsPrimary)
                            .ThenBy(x =>
                                x.OrderIndex)
                            .Select(x =>
                                x.ThumbnailUrl
                                ?? x.MediumUrl
                                ?? x.OriginalUrl)
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
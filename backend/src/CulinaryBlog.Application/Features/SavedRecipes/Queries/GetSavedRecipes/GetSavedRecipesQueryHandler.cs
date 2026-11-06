using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.DTOs.Recipes;
using CulinaryBlog.Domain.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Features.SavedRecipes.Queries.GetSavedRecipes;

public sealed class GetSavedRecipesQueryHandler
    : IRequestHandler<GetSavedRecipesQuery, PagedResult<RecipeSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public GetSavedRecipesQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<RecipeSummaryDto>> Handle(
        GetSavedRecipesQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (!_currentUser.IsAuthenticated || userId is null)
        {
            throw new UnauthorizedException("Sign in to view saved recipes.");
        }

        var result = await _unitOfWork.SavedRecipes.GetPagedAsync(
            userId.Value,
            request.Page,
            request.PageSize,
            cancellationToken);

        var items = result.Items.Select(savedRecipe =>
        {
            var recipe = savedRecipe.Recipe;
            return new RecipeSummaryDto(
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
                recipe.CreatedAt);
        }).ToList();

        return new PagedResult<RecipeSummaryDto>(
            items,
            result.TotalCount,
            request.Page,
            request.PageSize);
    }
}

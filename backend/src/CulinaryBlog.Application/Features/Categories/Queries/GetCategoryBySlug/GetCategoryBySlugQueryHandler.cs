using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.DTOs.Categories;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.DTOs.Recipes;
using CulinaryBlog.Domain.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Features.Categories.Queries.GetCategoryBySlug;

public sealed class GetCategoryBySlugQueryHandler
    : IRequestHandler<GetCategoryBySlugQuery, CategoryDetailDto>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IRecipeRepository _recipeRepository;
    private readonly ICurrentUser _currentUser;

    public GetCategoryBySlugQueryHandler(
        ICategoryRepository categoryRepository,
        IRecipeRepository recipeRepository,
        ICurrentUser currentUser)
    {
        _categoryRepository = categoryRepository;
        _recipeRepository = recipeRepository;
        _currentUser = currentUser;
    }

    public async Task<CategoryDetailDto> Handle(
        GetCategoryBySlugQuery request,
        CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetBySlugAsync(
            request.Slug,
            cancellationToken);

        if (category is null)
        {
            throw new NotFoundException(
                $"Category with slug '{request.Slug}' was not found.");
        }

        var recipeCount = await _categoryRepository
            .GetPublishedRecipeCountAsync(
                category.Id,
                cancellationToken);

        var categoryDto = new CategoryDto(
            category.Id,
            category.Name,
            category.Slug,
            category.Description,
            category.ImageUrl,
            category.OrderIndex,
            recipeCount);

        var recipes = await _recipeRepository.GetPagedForCategoryAsync(
            category.Id,
            request.Page,
            request.PageSize,
            _currentUser.UserId,
            _currentUser.IsAuthenticated,
            _currentUser.IsAdmin,
            cancellationToken);

        var recipeDtos = recipes.Items
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

        return new CategoryDetailDto(
            categoryDto,
            new PagedResult<RecipeSummaryDto>(
                recipeDtos,
                recipes.TotalCount,
                request.Page,
                request.PageSize));
    }
}

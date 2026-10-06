using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.DTOs.Categories;
using CulinaryBlog.Application.DTOs.Recipes;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Domain.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Features.Categories.Queries.GetCategoryBySlug;

public sealed class GetCategoryBySlugQueryHandler
    : IRequestHandler<GetCategoryBySlugQuery, CategoryDetailDto>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IRecipeRepository _recipeRepository;

    public GetCategoryBySlugQueryHandler(
        ICategoryRepository categoryRepository,
        IRecipeRepository recipeRepository)
    {
        _categoryRepository = categoryRepository;
        _recipeRepository = recipeRepository;
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

        var result = await _recipeRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            category.Id,
            difficulty: null,
            maxCookTime: null,
            sort: "-createdAt",
            currentUserId: null,
            isAuthenticated: false,
            isAdmin: false,
            cancellationToken);
        var recipes = result.Items.Select(recipe => new RecipeSummaryDto(
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
            recipe.CreatedAt)).ToList();

        var categoryDto = new CategoryDto(
            category.Id,
            category.Name,
            category.Slug,
            category.Description,
            category.ImageUrl,
            category.OrderIndex,
            result.TotalCount);

        return new CategoryDetailDto(
            categoryDto,
            new PagedResult<RecipeSummaryDto>(
                recipes,
                result.TotalCount,
                request.Page,
                request.PageSize));
    }
}
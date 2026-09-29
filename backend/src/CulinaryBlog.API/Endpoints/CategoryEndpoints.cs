using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.DTOs.Categories;
using CulinaryBlog.Application.DTOs.Recipes;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.API.Endpoints;

public static class CategoryEndpoints
{
    public static IEndpointRouteBuilder MapCategoryEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/categories")
            .WithTags("Categories");

        group.MapGet("/", GetCategories)
            .WithName("GetCategories");
        group.MapGet("/{slug}", GetCategoryBySlug)
            .WithName("GetCategoryBySlug");

        return endpoints;
    }

    private static async Task<IResult> GetCategories(
        CulinaryBlogDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var categories = await dbContext.Categories
            .AsNoTracking()
            .OrderBy(category => category.OrderIndex)
            .ThenBy(category => category.Name)
            .Select(category => new CategoryDto(
                category.Id,
                category.Name,
                category.Slug,
                category.Description,
                category.ImageUrl,
                category.OrderIndex,
                category.Recipes.Count(recipe =>
                    !recipe.IsDeleted &&
                    recipe.Status == RecipeStatus.Published)))
            .ToArrayAsync(cancellationToken);

        return Results.Ok(
            new ApiResponse<IReadOnlyList<CategoryDto>>(categories));
    }

    private static async Task<IResult> GetCategoryBySlug(
        string slug,
        [AsParameters] CategoryDetailRequest request,
        CulinaryBlogDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (request.Page < 1)
        {
            return ApiEndpointErrors.Unprocessable(
                "page phải lớn hơn hoặc bằng 1.",
                "page");
        }

        if (request.PageSize is < 1 or > 50)
        {
            return ApiEndpointErrors.Unprocessable(
                "pageSize phải nằm trong khoảng 1 đến 50.",
                "pageSize");
        }

        var category = await dbContext.Categories
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Slug == slug,
                cancellationToken);

        if (category is null)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Không tìm thấy danh mục",
                detail: $"Không có danh mục với slug '{slug}'.");
        }

        var recipeQuery = dbContext.Recipes
            .AsNoTracking()
            .Where(recipe =>
                recipe.CategoryId == category.Id &&
                recipe.Status == RecipeStatus.Published)
            .Include(recipe => recipe.Category)
            .Include(recipe => recipe.Images)
            .OrderByDescending(recipe => recipe.CreatedAt)
            .ThenBy(recipe => recipe.Id);

        var total = await recipeQuery.CountAsync(cancellationToken);
        var entities = await recipeQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
        var recipeItems = entities
            .Select(RecipeMapper.ToSummary)
            .ToArray();
        var pagedRecipes = PagedResult<RecipeSummaryDto>.Create(
            recipeItems,
            total,
            request.Page,
            request.PageSize);
        var categoryDto = new CategoryDto(
            category.Id,
            category.Name,
            category.Slug,
            category.Description,
            category.ImageUrl,
            category.OrderIndex,
            total);

        return Results.Ok(
            new ApiResponse<CategoryDetailDto>(
                new CategoryDetailDto(
                    categoryDto,
                    pagedRecipes)));
    }
}

public sealed class CategoryDetailRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 12;
}

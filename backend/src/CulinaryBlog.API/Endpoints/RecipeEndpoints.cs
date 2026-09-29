using System.Globalization;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.DTOs.Recipes;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.API.Endpoints;

public static class RecipeEndpoints
{
    public static IEndpointRouteBuilder MapRecipeEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/recipes")
            .WithTags("Recipes");

        group.MapGet("/", GetRecipes)
            .WithName("GetRecipes");
        group.MapGet("/search", SearchRecipes)
            .WithName("SearchRecipes");
        group.MapGet("/{slug}", GetRecipeBySlug)
            .WithName("GetRecipeBySlug");

        return endpoints;
    }

    private static async Task<IResult> GetRecipes(
        [AsParameters] RecipeListRequest request,
        CulinaryBlogDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateRequest(request);
        if (validationError is not null)
            return validationError;

        var recipes = dbContext.Recipes
            .AsNoTracking()
            .Where(recipe => recipe.Status == RecipeStatus.Published)
            .Include(recipe => recipe.Category)
            .Include(recipe => recipe.Images)
            .AsQueryable();

        if (request.CategoryId.HasValue)
            recipes = recipes.Where(recipe =>
                recipe.CategoryId == request.CategoryId.Value);

        if (!string.IsNullOrWhiteSpace(request.Difficulty))
        {
            Enum.TryParse<DifficultyLevel>(
                request.Difficulty,
                true,
                out var difficulty);
            recipes = recipes.Where(recipe =>
                recipe.Difficulty == difficulty);
        }

        if (request.MaxCookTime.HasValue)
            recipes = recipes.Where(recipe =>
                recipe.CookTimeMinutes <= request.MaxCookTime.Value);

        if (request.MinServings.HasValue)
            recipes = recipes.Where(recipe =>
                recipe.Servings >= request.MinServings.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = $"%{request.Search.Trim()}%";
            recipes = recipes.Where(recipe =>
                EF.Functions.ILike(recipe.Title, pattern) ||
                EF.Functions.ILike(recipe.Description, pattern));
        }

        recipes = ApplySorting(
            recipes,
            request.SortBy,
            request.SortOrder);

        return await CreatePagedResponse(
            recipes,
            request.Page,
            request.PageSize,
            cancellationToken);
    }

    private static async Task<IResult> SearchRecipes(
        [AsParameters] RecipeSearchRequest request,
        CulinaryBlogDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Q) ||
            request.Q.Trim().Length < 2)
        {
            return ApiEndpointErrors.Unprocessable(
                "Từ khóa tìm kiếm phải có ít nhất 2 ký tự.",
                "q");
        }

        var listRequest = new RecipeListRequest
        {
            Page = request.Page,
            PageSize = request.PageSize,
            CategoryId = request.CategoryId,
            Difficulty = request.Difficulty,
            MaxCookTime = request.MaxCookTime,
            MinServings = request.MinServings,
            SortBy = request.SortBy,
            SortOrder = request.SortOrder,
            Search = request.Q.Trim()
        };

        return await GetRecipes(
            listRequest,
            dbContext,
            cancellationToken);
    }

    private static async Task<IResult> GetRecipeBySlug(
        string slug,
        CulinaryBlogDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var recipe = await dbContext.Recipes
            .AsNoTracking()
            .Where(item => item.Status == RecipeStatus.Published)
            .Include(item => item.Category)
            .Include(item => item.Ingredients)
            .Include(item => item.Steps)
            .Include(item => item.Images)
            .SingleOrDefaultAsync(
                item => item.Slug == slug,
                cancellationToken);

        if (recipe is null)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Không tìm thấy công thức",
                detail: $"Không có công thức công khai với slug '{slug}'.");
        }

        return Results.Ok(
            new ApiResponse<RecipeDetailDto>(
                RecipeMapper.ToDetail(recipe)));
    }

    private static IResult? ValidateRequest(RecipeListRequest request)
    {
        if (request.Page < 1)
            return ApiEndpointErrors.Unprocessable(
                "page phải lớn hơn hoặc bằng 1.",
                "page");

        if (request.PageSize is < 1 or > 50)
            return ApiEndpointErrors.Unprocessable(
                "pageSize phải nằm trong khoảng 1 đến 50.",
                "pageSize");

        if (request.MaxCookTime < 0)
            return ApiEndpointErrors.Unprocessable(
                "maxCookTime không được âm.",
                "maxCookTime");

        if (request.MinServings < 1)
            return ApiEndpointErrors.Unprocessable(
                "minServings phải lớn hơn hoặc bằng 1.",
                "minServings");

        if (!string.IsNullOrWhiteSpace(request.Difficulty) &&
            !Enum.TryParse<DifficultyLevel>(
                request.Difficulty,
                true,
                out _))
        {
            return ApiEndpointErrors.Unprocessable(
                "difficulty phải là Easy, Medium, Hard hoặc Expert.",
                "difficulty");
        }

        var allowedSortFields = new[]
        {
            "title",
            "createdAt",
            "cookTimeMinutes"
        };

        if (!string.IsNullOrWhiteSpace(request.SortBy) &&
            !allowedSortFields.Contains(
                request.SortBy,
                StringComparer.OrdinalIgnoreCase))
        {
            return ApiEndpointErrors.Unprocessable(
                "sortBy chỉ nhận title, createdAt hoặc cookTimeMinutes.",
                "sortBy");
        }

        if (!string.IsNullOrWhiteSpace(request.SortOrder) &&
            !request.SortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase) &&
            !request.SortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase))
        {
            return ApiEndpointErrors.Unprocessable(
                "sortOrder chỉ nhận asc hoặc desc.",
                "sortOrder");
        }

        return null;
    }

    private static IQueryable<Recipe> ApplySorting(
        IQueryable<Recipe> query,
        string? sortBy,
        string? sortOrder)
    {
        var field = string.IsNullOrWhiteSpace(sortBy)
            ? "createdAt"
            : sortBy;
        var descending = string.IsNullOrWhiteSpace(sortOrder)
            ? field.Equals("createdAt", StringComparison.OrdinalIgnoreCase)
            : sortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase);

        return (field.ToLowerInvariant(), descending) switch
        {
            ("title", false) => query
                .OrderBy(recipe => recipe.Title)
                .ThenBy(recipe => recipe.Id),
            ("title", true) => query
                .OrderByDescending(recipe => recipe.Title)
                .ThenBy(recipe => recipe.Id),
            ("cooktimeminutes", false) => query
                .OrderBy(recipe => recipe.CookTimeMinutes)
                .ThenBy(recipe => recipe.Id),
            ("cooktimeminutes", true) => query
                .OrderByDescending(recipe => recipe.CookTimeMinutes)
                .ThenBy(recipe => recipe.Id),
            ("createdat", false) => query
                .OrderBy(recipe => recipe.CreatedAt)
                .ThenBy(recipe => recipe.Id),
            _ => query
                .OrderByDescending(recipe => recipe.CreatedAt)
                .ThenBy(recipe => recipe.Id)
        };
    }

    private static async Task<IResult> CreatePagedResponse(
        IQueryable<Recipe> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var total = await query.CountAsync(cancellationToken);
        var entities = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        var items = entities
            .Select(RecipeMapper.ToSummary)
            .ToArray();
        var paged = PagedResult<RecipeSummaryDto>.Create(
            items,
            total,
            page,
            pageSize);

        return Results.Ok(
            new ApiPagedResponse<RecipeSummaryDto>(
                paged.Items,
                new PaginationMeta(
                    paged.Page,
                    paged.PageSize,
                    paged.TotalCount,
                    paged.TotalPages,
                    paged.HasNextPage,
                    paged.HasPreviousPage)));
    }
}

public sealed class RecipeListRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 12;
    public Guid? CategoryId { get; init; }
    public string? Difficulty { get; init; }
    public int? MaxCookTime { get; init; }
    public int? MinServings { get; init; }
    public string? SortBy { get; init; }
    public string? SortOrder { get; init; }
    public string? Search { get; init; }
}

public sealed class RecipeSearchRequest
{
    public string? Q { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 12;
    public Guid? CategoryId { get; init; }
    public string? Difficulty { get; init; }
    public int? MaxCookTime { get; init; }
    public int? MinServings { get; init; }
    public string? SortBy { get; init; }
    public string? SortOrder { get; init; }
}

internal static class RecipeMapper
{
    public static RecipeSummaryDto ToSummary(Recipe recipe)
    {
        return new RecipeSummaryDto(
            recipe.Id,
            recipe.Title,
            recipe.Slug,
            recipe.Description,
            recipe.Description,
            GetPrimaryImageUrl(recipe),
            recipe.PrepTimeMinutes,
            recipe.CookTimeMinutes,
            recipe.Servings,
            recipe.Difficulty.ToString(),
            recipe.Category.Name,
            recipe.Category.Slug,
            "Culinary Blog",
            null,
            "Tác giả",
            recipe.CreatedAt,
            0,
            0,
            false,
            0,
            false);
    }

    public static RecipeDetailDto ToDetail(Recipe recipe)
    {
        return new RecipeDetailDto(
            recipe.Id,
            recipe.Title,
            recipe.Slug,
            recipe.Description,
            recipe.Description,
            GetPrimaryImageUrl(recipe),
            recipe.PrepTimeMinutes,
            recipe.CookTimeMinutes,
            recipe.Servings,
            recipe.Difficulty.ToString(),
            recipe.Status.ToString(),
            recipe.Category.Name,
            recipe.Category.Slug,
            "Culinary Blog",
            null,
            "Tác giả",
            recipe.CreatedAt,
            0,
            0,
            false,
            0,
            false,
            recipe.Ingredients
                .OrderBy(item => item.OrderIndex)
                .Select(item => new RecipeIngredientDto(
                    item.Id,
                    item.Name,
                    ParseQuantity(item.Quantity),
                    item.Unit,
                    item.Notes,
                    item.OrderIndex))
                .ToArray(),
            recipe.Steps
                .OrderBy(item => item.StepNumber)
                .Select(item => new RecipeStepDto(
                    item.Id,
                    item.StepNumber,
                    item.Title,
                    item.Description,
                    item.TimerMinutes,
                    item.ImageUrl))
                .ToArray(),
            recipe.Images
                .OrderByDescending(item => item.IsPrimary)
                .ThenBy(item => item.OrderIndex)
                .Select(item => new RecipeImageDto(
                    item.Id,
                    item.OriginalUrl,
                    item.MediumUrl,
                    item.ThumbnailUrl,
                    item.AltText,
                    item.IsPrimary,
                    item.OrderIndex))
                .ToArray());
    }

    private static string? GetPrimaryImageUrl(Recipe recipe)
    {
        var image = recipe.Images
            .OrderByDescending(item => item.IsPrimary)
            .ThenBy(item => item.OrderIndex)
            .FirstOrDefault();

        return image?.ThumbnailUrl ??
            image?.MediumUrl ??
            image?.OriginalUrl;
    }

    private static decimal? ParseQuantity(string? quantity)
    {
        if (decimal.TryParse(
            quantity,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var invariantValue))
        {
            return invariantValue;
        }

        if (decimal.TryParse(quantity, out var currentValue))
            return currentValue;

        return null;
    }
}

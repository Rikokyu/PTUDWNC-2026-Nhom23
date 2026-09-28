using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Recipes.Queries.GetRecipeBySlug;
using CulinaryBlog.Application.Features.Recipes.Queries.GetRecipes;
using CulinaryBlog.Domain.Enums;
using MediatR;

namespace CulinaryBlog.API.Endpoints;

public static class RecipeEndpoints
{
    public static IEndpointRouteBuilder MapRecipeEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group =
            endpoints
                .MapGroup("/api/v1/recipes")
                .WithTags("Recipes");

        group
            .MapGet(
                "",
                GetRecipesAsync)
            .CacheOutput("RecipeList");

        group
            .MapGet(
                "/{slug}",
                GetRecipeBySlugAsync)
            .CacheOutput("RecipeDetail");

        return endpoints;
    }

    private static async Task<IResult> GetRecipesAsync(
        HttpRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query =
            request.Query;

        if (!TryParsePositiveInt(
                query["page"],
                1,
                out var page))
        {
            throw new ValidationException(
                "page must be a positive integer.");
        }

        if (!TryParsePositiveInt(
                query["pageSize"],
                12,
                out var pageSize))
        {
            throw new ValidationException(
                "pageSize must be a positive integer.");
        }

        if (pageSize > 50)
        {
            throw new ValidationException(
                "pageSize cannot be greater than 50.");
        }

        Guid? categoryId = null;

        var categoryValue =
            query["categoryId"].FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(categoryValue))
        {
            if (!Guid.TryParse(
                    categoryValue,
                    out var parsedCategoryId))
            {
                throw new ValidationException(
                    "categoryId must be a valid GUID.");
            }

            categoryId = parsedCategoryId;
        }

        DifficultyLevel? difficulty = null;

        var difficultyValue =
            query["difficulty"].FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(
                difficultyValue))
        {
            if (!Enum.TryParse<DifficultyLevel>(
                    difficultyValue,
                    true,
                    out var parsedDifficulty))
            {
                throw new ValidationException(
                    "difficulty must be Easy, Medium, Hard or Expert.");
            }

            difficulty = parsedDifficulty;
        }

        int? maxCookTime = null;

        var maxCookTimeValue =
            query["maxCookTime"].FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(
                maxCookTimeValue))
        {
            if (!int.TryParse(
                    maxCookTimeValue,
                    out var parsedMaxCookTime)
                || parsedMaxCookTime < 0)
            {
                throw new ValidationException(
                    "maxCookTime must be a non-negative integer.");
            }

            maxCookTime = parsedMaxCookTime;
        }

        var sort =
            query["sort"].FirstOrDefault()
            ?? "-createdAt";

        var allowedSorts =
            new[]
            {
                "-createdAt",
                "createdAt",
                "title",
                "cookTime",
                "-cookTime"
            };

        if (!allowedSorts.Contains(
                sort,
                StringComparer.OrdinalIgnoreCase))
        {
            throw new ValidationException(
                "Invalid sort value.");
        }

        var requestQuery =
            new GetRecipesQuery(
                page,
                pageSize,
                categoryId,
                difficulty,
                maxCookTime,
                sort);

        var result =
            await sender.Send(
                requestQuery,
                cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetRecipeBySlugAsync(
        string slug,
        ISender sender,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new ValidationException(
                "slug is required.");
        }

        var query =
            new GetRecipeBySlugQuery(slug);

        var result =
            await sender.Send(
                query,
                cancellationToken);

        return Results.Ok(result);
    }

    private static bool TryParsePositiveInt(
        string? value,
        int defaultValue,
        out int result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = defaultValue;
            return true;
        }

        return int.TryParse(value, out result)
               && result > 0;
    }
}
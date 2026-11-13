using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.DTOs.Recipes;
using CulinaryBlog.Application.Features.Recipes.Commands;
using CulinaryBlog.Application.Features.Recipes.Queries.GetRecipeBySlug;
using CulinaryBlog.Application.Features.Recipes.Queries.GetRecipes;
using CulinaryBlog.Application.Features.Recipes.Queries.SearchRecipes;
using CulinaryBlog.Domain.Enums;
using Asp.Versioning;
using Asp.Versioning.Builder;
using MediatR;
using Microsoft.AspNetCore.OutputCaching;

namespace CulinaryBlog.API.Endpoints;

public static class RecipeEndpoints
{
    public static IEndpointRouteBuilder MapRecipeEndpoints(
        this IEndpointRouteBuilder endpoints,
        ApiVersionSet versionSet)
    {
        var group =
            endpoints
                .MapGroup("/api/v1/recipes")
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(new ApiVersion(1, 0))
                .WithTags("Recipes")
                .RequireRateLimiting("Api");

        group
            .MapGet(
                "",
                (HttpRequest request, ISender sender, CancellationToken cancellationToken) =>
                    GetRecipesAsync(
                        request,
                        sender,
                        cancellationToken,
                        ownRecipesOnly: false,
                        includeAllStatuses: false))
            .CacheOutput("RecipeList");

        group
            .MapGet(
                "/search",
                async (
                    HttpRequest request,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    static int ReadPagination(HttpRequest request, string name, int fallback)
                    {
                        if (!request.Query.TryGetValue(name, out var values))
                            return fallback;
                        if (values.Count != 1 || !int.TryParse(values[0],
                            System.Globalization.NumberStyles.Integer,
                            System.Globalization.CultureInfo.InvariantCulture, out var value)
                            || value < 1)
                            throw new ValidationException($"{name} must be a positive integer.");
                        return value;
                    }

                    var q = request.Query["q"].FirstOrDefault();
                    var page = ReadPagination(request, "page", 1);
                    var pageSize = ReadPagination(request, "pageSize", 10);
                    int? ReadFilter(string name, int minimum)
                    {
                        if (!request.Query.ContainsKey(name)) return null;
                        var values = request.Query[name];
                        if (values.Count != 1 || !int.TryParse(values[0], out var value) || value < minimum)
                            throw new ValidationException($"{name} must be an integer >= {minimum} occurring once.");
                        return value;
                    }
                    Guid? categoryId = null;
                    if (request.Query.ContainsKey("categoryId"))
                    {
                        var values = request.Query["categoryId"];
                        if (values.Count != 1 || !Guid.TryParse(values[0], out var id))
                            throw new ValidationException("categoryId must be a single GUID.");
                        categoryId = id;
                    }
                    DifficultyLevel? difficulty = null;
                    if (request.Query.ContainsKey("difficulty"))
                    {
                        var values = request.Query["difficulty"];
                        if (values.Count != 1 || !Enum.TryParse<DifficultyLevel>(values[0], true, out var level)
                            || !Enum.IsDefined(level))
                            throw new ValidationException("Invalid difficulty.");
                        difficulty = level;
                    }
                    var result = await sender.Send(
                        new SearchRecipesQuery(q ?? string.Empty, page, pageSize,
                            categoryId, difficulty, ReadFilter("maxCookTime", 0), ReadFilter("minServings", 1)),
                        cancellationToken);
                    return Results.Ok(result);
                })
            .CacheOutput("RecipeSearch")
            .WithSummary("Search published recipes by title, description, instructions, or ingredients.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group
            .MapGet(
                "/{slug}",
                GetRecipeBySlugAsync)
            .CacheOutput("RecipeDetail");

        group.MapPost(
            "",
            async (
                CreateRecipeRequest request,
                ISender sender,
                IOutputCacheStore outputCacheStore,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new CreateRecipeCommand(
                        request.Title,
                        request.Description,
                        request.Instructions,
                        request.CategoryId,
                        request.PrepTimeMinutes,
                        request.CookTimeMinutes,
                        request.Servings,
                        request.Difficulty,
                        request.Nutrition,
                        request.Ingredients ?? Array.Empty<RecipeIngredientInput>(),
                        request.Steps ?? Array.Empty<RecipeStepInput>()),
                    cancellationToken);

                await outputCacheStore.SafeEvictByTagAsync(
                    "recipes",
                    cancellationToken);

                var createdRecipe = await sender.Send(
                    new GetRecipeBySlugQuery(result.Slug),
                    cancellationToken);

                return Results.Created(
                    $"/api/v1/recipes/{result.Slug}",
                    new { data = createdRecipe });
            })
            .RequireAuthorization("AuthorOrAdmin");

        group.MapPut(
            "/{recipeId:guid}",
            async (
                Guid recipeId,
                UpdateRecipeRequest request,
                ISender sender,
                IOutputCacheStore outputCacheStore,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new UpdateRecipeCommand(
                        recipeId,
                        request.Title,
                        request.Description,
                        request.Instructions,
                        request.CategoryId,
                        request.PrepTimeMinutes,
                        request.CookTimeMinutes,
                        request.Servings,
                        request.Difficulty,
                        request.RowVersion,
                        request.Nutrition),
                    cancellationToken);

                await outputCacheStore.SafeEvictByTagAsync(
                    "recipes",
                    cancellationToken);
                await outputCacheStore.SafeEvictByTagAsync(
                    $"recipe:{result.Slug}",
                    cancellationToken);

                var updatedRecipe = await sender.Send(
                    new GetRecipeBySlugQuery(result.Slug),
                    cancellationToken);

                return Results.Ok(new { data = updatedRecipe });
            })
            .RequireAuthorization("AuthorOrAdmin");

        group.MapPatch(
            "/{recipeId:guid}/publish",
            (
                Guid recipeId,
                ISender sender,
                IOutputCacheStore outputCacheStore,
                CancellationToken cancellationToken) =>
                SetStatusAsync(
                    recipeId,
                    RecipeStatus.Published,
                    sender,
                    outputCacheStore,
                    cancellationToken))
            .RequireAuthorization("AuthorOrAdmin");

        group.MapPatch(
            "/{recipeId:guid}/unpublish",
            (
                Guid recipeId,
                ISender sender,
                IOutputCacheStore outputCacheStore,
                CancellationToken cancellationToken) =>
                SetStatusAsync(
                    recipeId,
                    RecipeStatus.Draft,
                    sender,
                    outputCacheStore,
                    cancellationToken))
            .RequireAuthorization("AuthorOrAdmin");

        group.MapPatch(
            "/{recipeId:guid}/archive",
            (
                Guid recipeId,
                ISender sender,
                IOutputCacheStore outputCacheStore,
                CancellationToken cancellationToken) =>
                SetStatusAsync(
                    recipeId,
                    RecipeStatus.Archived,
                    sender,
                    outputCacheStore,
                    cancellationToken))
            .RequireAuthorization("AuthorOrAdmin");

        group.MapDelete(
            "/{recipeId:guid}",
            async (
                Guid recipeId,
                ISender sender,
                IOutputCacheStore outputCacheStore,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new DeleteRecipeCommand(recipeId),
                    cancellationToken);

                await outputCacheStore.SafeEvictByTagAsync(
                    "recipes",
                    cancellationToken);
                await outputCacheStore.SafeEvictByTagAsync(
                    $"recipe:{result.Slug}",
                    cancellationToken);

                return Results.NoContent();
            })
            .RequireAuthorization("AuthorOrAdmin");

        endpoints.MapGet(
            "/api/v1/me/recipes",
            async (
                HttpRequest request,
                ISender sender,
                CulinaryBlog.Application.Common.Interfaces.ICurrentUser currentUser,
                CancellationToken cancellationToken) =>
            {
                if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
                {
                    throw new ForbiddenException(
                        "Sign in to view your recipes.");
                }

                return await GetRecipesAsync(
                    request,
                    sender,
                    cancellationToken,
                    ownRecipesOnly: true,
                    includeAllStatuses: false);
            })
            .RequireAuthorization("AuthorOrAdmin");

        endpoints.MapGet(
            "/api/v1/admin/recipes",
            async (
                HttpRequest request,
                ISender sender,
                CulinaryBlog.Application.Common.Interfaces.ICurrentUser currentUser,
                CancellationToken cancellationToken) =>
            {
                if (!currentUser.IsAdmin)
                {
                    throw new ForbiddenException(
                        "Administrator access is required.");
                }

                return await GetRecipesAsync(
                    request,
                    sender,
                    cancellationToken,
                    ownRecipesOnly: false,
                    includeAllStatuses: true);
            })
            .RequireAuthorization("Admin");

        return endpoints;
    }

    private static async Task<IResult> SetStatusAsync(
        Guid recipeId,
        RecipeStatus status,
        ISender sender,
        IOutputCacheStore outputCacheStore,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new SetRecipeStatusCommand(recipeId, status),
            cancellationToken);

        await outputCacheStore.SafeEvictByTagAsync(
            "recipes",
            cancellationToken);
        await outputCacheStore.SafeEvictByTagAsync(
            $"recipe:{result.Slug}",
            cancellationToken);

        var recipe = await sender.Send(
            new GetRecipeBySlugQuery(result.Slug),
            cancellationToken);

        return Results.Ok(new { data = recipe });
    }

    private static async Task<IResult> GetRecipesAsync(
        HttpRequest request,
        ISender sender,
        CancellationToken cancellationToken,
        bool ownRecipesOnly,
        bool includeAllStatuses)
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

        int? minServings = null;
        if (query.ContainsKey("minServings"))
        {
            if (query["minServings"].Count != 1
                || !int.TryParse(query["minServings"].FirstOrDefault(), out var parsedMinServings)
                || parsedMinServings < 1)
                throw new ValidationException("minServings must be a positive integer occurring once.");
            minServings = parsedMinServings;
        }

        var search = query["search"].FirstOrDefault()?.Trim();

        if (search?.Length > 100)
        {
            throw new ValidationException(
                "search cannot be longer than 100 characters.");
        }

        var sort =
            query["sort"].FirstOrDefault()
            ?? "-createdAt";

        if (query.ContainsKey("sortBy") || query.ContainsKey("sortOrder"))
        {
            if (query["sortBy"].Count > 1 || query["sortOrder"].Count > 1)
                throw new ValidationException("Sort parameters must occur only once.");
            var sortBy = query["sortBy"].FirstOrDefault() ?? "createdAt";
            var sortOrder = query["sortOrder"].FirstOrDefault() ?? "desc";
            if (sortBy is not ("title" or "createdAt" or "cookTime")
                || sortOrder is not ("asc" or "desc"))
                throw new ValidationException("sortBy must be title, createdAt or cookTime; sortOrder must be asc or desc.");
            sort = (sortOrder == "desc" ? "-" : "") + sortBy;
        }

        var allowedSorts =
            new[]
            {
                "-createdAt",
                "createdAt",
                "title",
                "-title",
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
                search,
                sort,
                ownRecipesOnly,
                includeAllStatuses,
                minServings);

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

public sealed record CreateRecipeRequest(
    string Title,
    string Description,
    Guid CategoryId,
    int PrepTimeMinutes,
    int CookTimeMinutes,
    int Servings,
    DifficultyLevel Difficulty,
    string? Instructions = null,
    RecipeNutritionDto? Nutrition = null,
    IReadOnlyList<RecipeIngredientInput>? Ingredients = null,
    IReadOnlyList<RecipeStepInput>? Steps = null);

public sealed record UpdateRecipeRequest(
    string Title,
    string Description,
    Guid CategoryId,
    int PrepTimeMinutes,
    int CookTimeMinutes,
    int Servings,
    DifficultyLevel Difficulty,
    uint RowVersion,
    string? Instructions = null,
    RecipeNutritionDto? Nutrition = null);

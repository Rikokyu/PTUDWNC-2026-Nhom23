using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.DTOs.Recipes;
using CulinaryBlog.Application.Features.Recipes.Commands;
using CulinaryBlog.Application.Features.Recipes.Queries.GetRecipeBySlug;
using CulinaryBlog.Application.Features.Recipes.Queries.GetRecipes;
using CulinaryBlog.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.OutputCaching;

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
                "/{slug}",
                GetRecipeBySlugAsync)
            .CacheOutput("RecipeDetail")
            .CacheOutput("RecipeSlug");

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

                await outputCacheStore.EvictByTagAsync(
                    "recipes",
                    cancellationToken);

                var createdRecipe = await sender.Send(
                    new GetRecipeBySlugQuery(result.Slug),
                    cancellationToken);

                return Results.Created(
                    $"/api/v1/recipes/{result.Slug}",
                    new { data = createdRecipe });
            })
            .RequireAuthorization();

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

                await outputCacheStore.EvictByTagAsync(
                    "recipes",
                    cancellationToken);
                await outputCacheStore.EvictByTagAsync(
                    $"recipe:{result.Slug}",
                    cancellationToken);

                var updatedRecipe = await sender.Send(
                    new GetRecipeBySlugQuery(result.Slug),
                    cancellationToken);

                return Results.Ok(new { data = updatedRecipe });
            })
            .RequireAuthorization();

        group.MapPatch(
            "/{recipeId:guid}/publish",
            (Guid recipeId, ISender sender, CancellationToken cancellationToken) =>
                SetStatusAsync(
                    recipeId,
                    RecipeStatus.Published,
                    sender,
                    cancellationToken));

        group.MapPatch(
            "/{recipeId:guid}/unpublish",
            (Guid recipeId, ISender sender, CancellationToken cancellationToken) =>
                SetStatusAsync(
                    recipeId,
                    RecipeStatus.Draft,
                    sender,
                    cancellationToken));

        group.MapPatch(
            "/{recipeId:guid}/archive",
            (Guid recipeId, ISender sender, CancellationToken cancellationToken) =>
                SetStatusAsync(
                    recipeId,
                    RecipeStatus.Archived,
                    sender,
                    cancellationToken));

        group.MapDelete(
            "/{recipeId:guid}",
            async (
                Guid recipeId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                await sender.Send(
                    new DeleteRecipeCommand(recipeId),
                    cancellationToken);
                return Results.NoContent();
            });

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
            });

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
            });

        return endpoints;
    }

    private static async Task<IResult> SetStatusAsync(
        Guid recipeId,
        RecipeStatus status,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new SetRecipeStatusCommand(recipeId, status),
            cancellationToken);
        return Results.NoContent();
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
                sort,
                ownRecipesOnly,
                includeAllStatuses);

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
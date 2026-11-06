using Asp.Versioning;
using Asp.Versioning.Builder;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.SavedRecipes.Commands.RemoveSavedRecipe;
using CulinaryBlog.Application.Features.SavedRecipes.Commands.SaveRecipe;
using CulinaryBlog.Application.Features.SavedRecipes.Queries.GetSavedRecipeIds;
using CulinaryBlog.Application.Features.SavedRecipes.Queries.GetSavedRecipes;
using MediatR;

namespace CulinaryBlog.API.Endpoints;

public static class SavedRecipeEndpoints
{
    public static IEndpointRouteBuilder MapSavedRecipeEndpoints(
        this IEndpointRouteBuilder endpoints,
        ApiVersionSet versionSet)
    {
        var group = endpoints
            .MapGroup("/api/v1/saved-recipes")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(new ApiVersion(1, 0))
            .WithTags("Saved Recipes")
            .RequireRateLimiting("Api")
            .RequireAuthorization("AuthorOrAdmin");

        group.MapPost(
                "/{recipeId:guid}",
                async (
                    Guid recipeId,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    await sender.Send(
                        new SaveRecipeCommand(recipeId),
                        cancellationToken);
                    return Results.NoContent();
                })
            .WithName("SaveRecipe")
            .WithSummary("Save a published recipe to the current user's saved list.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet(
                "",
                async (
                    int? page,
                    int? pageSize,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var currentPage = page ?? 1;
                    var currentPageSize = pageSize ?? 12;
                    ValidatePagination(currentPage, currentPageSize);

                    var result = await sender.Send(
                        new GetSavedRecipesQuery(currentPage, currentPageSize),
                        cancellationToken);
                    return Results.Ok(result);
                })
            .WithName("GetSavedRecipes")
            .WithSummary("Get the current user's saved published recipes.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet(
                "/ids",
                async (
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var ids = await sender.Send(
                        new GetSavedRecipeIdsQuery(),
                        cancellationToken);
                    return Results.Ok(ids);
                })
            .WithName("GetSavedRecipeIds")
            .WithSummary("Get saved recipe IDs for bookmark states in recipe lists.")
            .Produces<Guid[]>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapDelete(
                "/{recipeId:guid}",
                async (
                    Guid recipeId,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    await sender.Send(
                        new RemoveSavedRecipeCommand(recipeId),
                        cancellationToken);
                    return Results.NoContent();
                })
            .WithName("RemoveSavedRecipe")
            .WithSummary("Remove a recipe from the current user's saved list.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return endpoints;
    }

    private static void ValidatePagination(int page, int pageSize)
    {
        if (page < 1)
        {
            throw new ValidationException("page must be a positive integer.");
        }

        if (pageSize is < 1 or > 50)
        {
            throw new ValidationException("pageSize must be between 1 and 50.");
        }
    }
}

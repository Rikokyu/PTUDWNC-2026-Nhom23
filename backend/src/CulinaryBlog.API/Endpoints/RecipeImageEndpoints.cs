using Asp.Versioning;
using Asp.Versioning.Builder;
using CulinaryBlog.Application.Features.Recipes.Commands;
using CulinaryBlog.Application.Features.Recipes.Commands.UploadRecipeImage;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace CulinaryBlog.API.Endpoints;

public static class RecipeImageEndpoints
{
    private const long MaximumMultipartRequestSize = 21 * 1024 * 1024;

    public static IEndpointRouteBuilder MapRecipeImageEndpoints(
        this IEndpointRouteBuilder endpoints,
        ApiVersionSet versionSet)
    {
        var group = endpoints
            .MapGroup("/api/v1/recipes/{recipeId:guid}/images")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(new ApiVersion(1, 0))
            .WithTags("Recipe Images")
            .RequireRateLimiting("Api")
            .RequireAuthorization();

        group.MapPost(
            "",
            async (
                Guid recipeId,
                [FromForm] UploadRecipeImageForm request,
                ISender sender,
                IOutputCacheStore outputCacheStore,
                CancellationToken cancellationToken) =>
            {
                if (request.File is null || request.File.Length == 0)
                {
                    throw new ArgumentException("Image file is required.");
                }

                if (request.File.Length > 20 * 1024 * 1024)
                {
                    throw new ArgumentException(
                        "Image size cannot exceed 20 MB.");
                }

                await using var stream = new MemoryStream();
                await request.File.CopyToAsync(stream, cancellationToken);
                var image = await sender.Send(
                    new UploadRecipeImageCommand(
                        recipeId,
                        stream.ToArray(),
                        request.File.ContentType,
                        request.AltText,
                        request.IsPrimary),
                    cancellationToken);

                await outputCacheStore.EvictByTagAsync(
                    "recipes",
                    cancellationToken);
                return Results.Created(
                    $"/api/v1/recipes/{recipeId}/images/{image.Id}",
                    new { data = image, url = image.OriginalUrl, image.IsPrimary });
            })
            .Accepts<UploadRecipeImageForm>("multipart/form-data")
            .Produces(StatusCodes.Status201Created)
            .WithMetadata(new RequestSizeLimitAttribute(MaximumMultipartRequestSize))
            .RequireRateLimiting("Upload")
            .DisableAntiforgery()
            .WithSummary("Upload an image for a recipe.");

        group.MapPatch(
            "/{imageId:guid}",
            async (
                Guid recipeId,
                Guid imageId,
                RecipeImageUpdateRequest request,
                ISender sender,
                IOutputCacheStore outputCacheStore,
                CancellationToken cancellationToken) =>
            {
                await sender.Send(
                    new UpdateRecipeImageCommand(
                        recipeId,
                        imageId,
                        request.AltText,
                        request.IsPrimary),
                    cancellationToken);
                await outputCacheStore.EvictByTagAsync(
                    "recipes",
                    cancellationToken);
                return Results.NoContent();
            })
            .WithSummary("Update a recipe image's alt text.")
            .Produces(StatusCodes.Status204NoContent);

        group.MapPatch(
            "/{imageId:guid}/primary",
            async (
                Guid recipeId,
                Guid imageId,
                ISender sender,
                IOutputCacheStore outputCacheStore,
                CancellationToken cancellationToken) =>
            {
                await sender.Send(
                    new UpdateRecipeImageCommand(
                        recipeId,
                        imageId,
                        null,
                        true),
                    cancellationToken);
                await outputCacheStore.EvictByTagAsync(
                    "recipes",
                    cancellationToken);
                return Results.Ok(new { id = imageId, isPrimary = true });
            })
            .WithSummary("Set a recipe image as the primary image.")
            .Produces(StatusCodes.Status200OK);

        group.MapDelete(
            "/{imageId:guid}",
            async (
                Guid recipeId,
                Guid imageId,
                ISender sender,
                IOutputCacheStore outputCacheStore,
                CancellationToken cancellationToken) =>
            {
                await sender.Send(
                    new DeleteRecipeImageCommand(recipeId, imageId),
                    cancellationToken);
                await outputCacheStore.EvictByTagAsync(
                    "recipes",
                    cancellationToken);
                return Results.NoContent();
            })
            .WithSummary("Delete a recipe image and queue its stored files for cleanup.")
            .Produces(StatusCodes.Status204NoContent);

        return endpoints;
    }

}

public sealed class UploadRecipeImageForm
{
    public IFormFile File { get; init; } = null!;

    public string? AltText { get; init; }

    public bool? IsPrimary { get; init; }
}

public sealed record RecipeImageUpdateRequest(
    string? AltText,
    bool? IsPrimary);

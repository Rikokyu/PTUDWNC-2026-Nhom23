using CulinaryBlog.Application.DTOs.Recipes;
using CulinaryBlog.Application.Features.Recipes.Commands.UploadRecipeImage;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace CulinaryBlog.API.Endpoints;

public static class RecipeImageEndpoints
{
    private const long MaxFileSize = 5 * 1024 * 1024;

    public static IEndpointRouteBuilder MapRecipeImageEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                "/api/v1/recipes/{id:guid}/images",
                UploadRecipeImageAsync)
            .WithName("UploadRecipeImage")
            .WithTags("Recipe Images")
            .RequireAuthorization()
            .DisableAntiforgery()
            .Produces<RecipeImageDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return endpoints;
    }

    private static async Task<IResult> UploadRecipeImageAsync(
        Guid id,
        IFormFile file,
        [FromForm] string? altText,
        [FromForm] bool? isPrimary,
        ISender sender,
        IOutputCacheStore outputCache,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            throw new ArgumentException("Image file is required.");
        }

        if (file.Length > MaxFileSize)
        {
            throw new ArgumentException(
                "Image size cannot exceed 5 MB.");
        }

        await using var stream = new MemoryStream(
            checked((int)file.Length));
        await file.CopyToAsync(stream, cancellationToken);

        var result = await sender.Send(
            new UploadRecipeImageCommand(
                id,
                stream.ToArray(),
                file.ContentType,
                altText,
                isPrimary),
            cancellationToken);

        await outputCache.EvictByTagAsync(
            "recipes",
            cancellationToken);

        return Results.Created(
            $"/api/v1/recipes/{id}/images/{result.Id}",
            result);
    }
}

using CulinaryBlog.Application.DTOs.Recipes;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes.Commands.UploadRecipeImage;

public sealed record UploadRecipeImageCommand(
    Guid RecipeId,
    byte[] Content,
    string ContentType,
    string? AltText,
    bool? IsPrimary) : IRequest<RecipeImageDto>;

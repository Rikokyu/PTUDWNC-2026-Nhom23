using CulinaryBlog.Application.Common.Authorization;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.DTOs.Recipes;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Application.Features.Recipes.Commands.UploadRecipeImage;

public sealed class UploadRecipeImageCommandHandler
    : IRequestHandler<UploadRecipeImageCommand, RecipeImageDto>
{
    private const int MaxFileSize = 5 * 1024 * 1024;

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IFileStorageService _fileStorage;
    private readonly IImageResizeJobScheduler _imageResizeJobs;
    private readonly ILogger<UploadRecipeImageCommandHandler> _logger;

    public UploadRecipeImageCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IFileStorageService fileStorage,
        IImageResizeJobScheduler imageResizeJobs,
        ILogger<UploadRecipeImageCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _fileStorage = fileStorage;
        _imageResizeJobs = imageResizeJobs;
        _logger = logger;
    }

    public async Task<RecipeImageDto> Handle(
        UploadRecipeImageCommand request,
        CancellationToken cancellationToken)
    {
        EnsureAuthenticated();
        ValidateFile(request.Content, request.ContentType, request.AltText);

        var recipe = await _unitOfWork.Recipes.GetByIdWithImagesAsync(
            request.RecipeId,
            cancellationToken);

        if (recipe is null)
        {
            throw new NotFoundException(
                $"Recipe with id '{request.RecipeId}' was not found.");
        }

        if (!_currentUser.IsAdmin
            && recipe.AuthorId != _currentUser.UserId)
        {
            throw new ForbiddenException(
                "Only the recipe owner or an administrator can upload images.");
        }

        var shouldBePrimary =
            recipe.Images.Count == 0 || request.IsPrimary == true;

        if (shouldBePrimary)
        {
            foreach (var currentImage in recipe.Images)
            {
                currentImage.IsPrimary = false;
                currentImage.UpdatedAt = DateTime.UtcNow;
            }
        }

        var extension = GetExtension(request.ContentType);
        var originalUrl = await _fileStorage.UploadAsync(
            request.Content,
            extension,
            $"recipes/{recipe.Id}",
            cancellationToken);

        var image = RecipeImage.Create(
            recipe.Id,
            originalUrl,
            NormalizeAltText(request.AltText),
            shouldBePrimary,
            recipe.Images.Count == 0
                ? 1
                : recipe.Images.Max(item => item.OrderIndex) + 1);

        recipe.Images.Add(image);
        // Explicitly mark the new dependent as Added; RecipeImage IDs are generated
        // in the domain, so graph discovery can otherwise treat it as an existing row.
        _unitOfWork.AddRecipeImage(image);

        try
        {
            // The recipe was loaded by this DbContext and remains tracked. Adding the
            // image to its collection is sufficient; marking the whole graph modified
            // would issue stale updates for existing images.
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await _fileStorage.DeleteAsync(
                originalUrl,
                CancellationToken.None);
            throw;
        }

        try
        {
            _imageResizeJobs.Enqueue(image.Id);
        }
        catch (Exception exception)
        {
            // Preserve successful upload behavior even if Hangfire is temporarily
            // unavailable; the original is already persisted and remains usable.
            _logger.LogError(
                exception,
                "Could not enqueue image resize job for image {ImageId}.",
                image.Id);
        }

        return new RecipeImageDto(
            image.Id,
            image.OriginalUrl,
            image.MediumUrl,
            image.ThumbnailUrl,
            image.AltText,
            image.IsPrimary,
            image.OrderIndex);
    }

    private void EnsureAuthenticated()
    {
        if (!_currentUser.IsAuthenticated)
        {
            throw new UnauthorizedException(
                "Authentication is required.");
        }
    }

    private static void ValidateFile(
        byte[] content,
        string contentType,
        string? altText)
    {
        if (content.Length == 0)
        {
            throw new ArgumentException("Image file is required.");
        }

        if (content.Length > MaxFileSize)
        {
            throw new ArgumentException(
                "Image size cannot exceed 5 MB.");
        }

        if (altText?.Length > 200)
        {
            throw new ArgumentException(
                "AltText cannot exceed 200 characters.");
        }

        if (!HasValidSignature(content, contentType))
        {
            throw new ArgumentException(
                "Image must be a valid JPEG, PNG, WebP or AVIF file.");
        }
    }

    private static bool HasValidSignature(
        byte[] content,
        string contentType)
    {
        return contentType.ToLowerInvariant() switch
        {
            "image/jpeg" => content.Length >= 3
                && content[0] == 0xFF
                && content[1] == 0xD8
                && content[2] == 0xFF,
            "image/png" => content.Length >= 8
                && content.AsSpan(0, 8).SequenceEqual(
                    new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            "image/webp" => content.Length >= 12
                && content.AsSpan(0, 4).SequenceEqual("RIFF"u8)
                && content.AsSpan(8, 4).SequenceEqual("WEBP"u8),
            "image/avif" => content.Length >= 12
                && content.AsSpan(4, 4).SequenceEqual("ftyp"u8)
                && (content.AsSpan(8, 4).SequenceEqual("avif"u8)
                    || content.AsSpan(8, 4).SequenceEqual("avis"u8)),
            _ => false
        };
    }

    private static string GetExtension(string contentType) =>
        contentType.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            "image/avif" => ".avif",
            _ => throw new ArgumentException("Unsupported image content type.")
        };

    private static string? NormalizeAltText(string? altText) =>
        string.IsNullOrWhiteSpace(altText)
            ? null
            : altText.Trim();
}

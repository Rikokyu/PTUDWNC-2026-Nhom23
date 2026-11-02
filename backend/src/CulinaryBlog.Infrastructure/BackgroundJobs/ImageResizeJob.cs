using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Interfaces;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Infrastructure.BackgroundJobs;

public sealed class ImageResizeJob(
    IUnitOfWork unitOfWork,
    IFileStorageService fileStorage,
    ILogger<ImageResizeJob> logger)
{
    [AutomaticRetry(
        Attempts = 3,
        OnAttemptsExceeded = AttemptsExceededAction.Fail,
        LogEvents = true)]
    public async Task ProcessAsync(
        Guid imageId,
        CancellationToken cancellationToken)
    {
        var image = await unitOfWork.Recipes.GetImageByIdAsync(
            imageId,
            cancellationToken);

        if (image is null)
        {
            logger.LogInformation(
                "Skipping image resize for deleted image {ImageId}.",
                imageId);
            return;
        }

        if (image.MediumUrl is not null
            && image.ThumbnailUrl is not null)
        {
            return;
        }

        var sourceBytes = await fileStorage.ReadAsync(
            image.OriginalUrl,
            cancellationToken);
        var (mediumBytes, thumbnailBytes) = await ImageVariantProcessor.CreateAsync(
            sourceBytes,
            cancellationToken);

        var folder = $"recipes/{image.RecipeId}";
        string? mediumUrl = null;
        string? thumbnailUrl = null;

        try
        {
            mediumUrl = await fileStorage.UploadAsync(
                mediumBytes,
                ".webp",
                folder,
                cancellationToken);
            thumbnailUrl = await fileStorage.UploadAsync(
                thumbnailBytes,
                ".webp",
                folder,
                cancellationToken);

            image.MediumUrl = mediumUrl;
            image.ThumbnailUrl = thumbnailUrl;
            image.UpdatedAt = DateTime.UtcNow;
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            if (thumbnailUrl is not null)
            {
                await fileStorage.DeleteAsync(
                    thumbnailUrl,
                    CancellationToken.None);
            }

            if (mediumUrl is not null)
            {
                await fileStorage.DeleteAsync(
                    mediumUrl,
                    CancellationToken.None);
            }

            throw;
        }

        logger.LogInformation(
            "Generated medium and thumbnail variants for recipe image {ImageId}.",
            imageId);
    }

}

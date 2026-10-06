using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.DTOs.Recipes;
using CulinaryBlog.Application.Features.Recipes.Commands;
using CulinaryBlog.Application.Features.Recipes.Queries.SearchRecipes;
using CulinaryBlog.Application.Features.Recipes.Queries.GetRecipeBySlug;
using CulinaryBlog.Application.Features.Recipes.Queries.GetRecipes;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.API.Middleware;
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
                GetRecipesAsync)
            .CacheOutput("RecipeList")
            .WithName("GetRecipes")
            .Produces<PagedResult<RecipeSummaryDto>>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/search", SearchRecipesAsync)
            .WithName("SearchRecipes")
            .Produces<PagedResult<RecipeSummaryDto>>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group
            .MapGet(
                "/{slug}",
                GetRecipeBySlugAsync)
            .CacheOutput("RecipeDetail")
            .WithName("GetRecipeBySlug")
            .Produces<RecipeDetailDto>()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("", CreateRecipeAsync)
            .RequireAuthorization(policy => policy.RequireRole("Author", "Admin"))
            .WithName("CreateRecipe")
            .Produces<RecipeDetailDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
        group.MapPut("/{id:guid}", UpdateRecipeAsync)
            .RequireAuthorization(policy => policy.RequireRole("Author", "Admin"))
            .WithName("UpdateRecipe")
            .Produces<RecipeDetailDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
        group.MapPatch("/{id:guid}/publish", PublishRecipeAsync)
            .RequireAuthorization(policy => policy.RequireRole("Author", "Admin"))
            .WithName("PublishRecipe")
            .Produces<RecipeDetailDto>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
        group.MapPatch("/{id:guid}/unpublish", UnpublishRecipeAsync)
            .RequireAuthorization(policy => policy.RequireRole("Author", "Admin"))
            .WithName("UnpublishRecipe")
            .Produces<RecipeDetailDto>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
        group.MapPatch("/{id:guid}/archive", ArchiveRecipeAsync)
            .RequireAuthorization(policy => policy.RequireRole("Author", "Admin"))
            .WithName("ArchiveRecipe")
            .Produces<RecipeDetailDto>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
        group.MapDelete("/{id:guid}", DeleteRecipeAsync)
            .RequireAuthorization(policy => policy.RequireRole("Author", "Admin"))
            .WithName("DeleteRecipe")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/ingredients", AddIngredientAsync)
            .RequireAuthorization(policy => policy.RequireRole("Author", "Admin"))
            .WithName("AddRecipeIngredient")
            .Produces<RecipeIngredientDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
        group.MapPut("/{id:guid}/ingredients/{ingredientId:guid}", UpdateIngredientAsync)
            .RequireAuthorization(policy => policy.RequireRole("Author", "Admin"))
            .WithName("UpdateRecipeIngredient")
            .Produces<RecipeIngredientDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
        group.MapDelete("/{id:guid}/ingredients/{ingredientId:guid}", DeleteIngredientAsync)
            .RequireAuthorization(policy => policy.RequireRole("Author", "Admin"))
            .WithName("DeleteRecipeIngredient")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/steps", AddStepAsync)
            .RequireAuthorization(policy => policy.RequireRole("Author", "Admin"))
            .WithName("AddRecipeStep")
            .Produces<RecipeStepDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
        group.MapPut("/{id:guid}/steps/{stepId:guid}", UpdateStepAsync)
            .RequireAuthorization(policy => policy.RequireRole("Author", "Admin"))
            .WithName("UpdateRecipeStep")
            .Produces<RecipeStepDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
        group.MapDelete("/{id:guid}/steps/{stepId:guid}", DeleteStepAsync)
            .RequireAuthorization(policy => policy.RequireRole("Author", "Admin"))
            .WithName("DeleteRecipeStep")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/images", UploadImageAsync)
            .RequireAuthorization(policy => policy.RequireRole("Author", "Admin"))
            .WithName("UploadRecipeImage")
            .Accepts<RecipeImageUploadRequest>("multipart/form-data")
            .Produces<RecipeImageDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable);
        group.MapPatch("/{id:guid}/images/{imageId:guid}", UpdateImageMetadataAsync)
            .RequireAuthorization(policy => policy.RequireRole("Author", "Admin"))
            .WithName("UpdateRecipeImageMetadata")
            .Produces<RecipeImageDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
        group.MapPatch("/{id:guid}/images/{imageId:guid}/primary", SetPrimaryImageAsync)
            .RequireAuthorization(policy => policy.RequireRole("Author", "Admin"))
            .WithName("SetPrimaryRecipeImage")
            .Produces<RecipeImageDto>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
        group.MapDelete("/{id:guid}/images/{imageId:guid}", DeleteImageAsync)
            .RequireAuthorization(policy => policy.RequireRole("Author", "Admin"))
            .WithName("DeleteRecipeImage")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable);

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

        var sort = query["sort"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(sort))
        {
            var sortBy = query["sortBy"].FirstOrDefault();
            var sortOrder = query["sortOrder"].FirstOrDefault() ?? "desc";
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var sortFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["name"] = "title",
                    ["title"] = "title",
                    ["createdAt"] = "createdAt",
                    ["cookTime"] = "cookTime",
                    ["cookTimeMinutes"] = "cookTime",
                    ["prepTime"] = "prepTime",
                    ["prepTimeMinutes"] = "prepTime",
                    ["categoryName"] = "categoryName"
                };

                if (!sortFields.TryGetValue(sortBy, out var sortField)
                    || sortOrder is not ("asc" or "desc"))
                {
                    throw new ValidationException("sortBy or sortOrder is invalid.");
                }

                sort = sortOrder == "desc" ? $"-{sortField}" : sortField;
            }
            else
            {
                sort = "-createdAt";
            }
        }

        var allowedSorts =
            new[]
            {
                "-createdAt",
                "createdAt",
                "title",
                "-title",
                "cookTime",
                "-cookTime",
                "prepTime",
                "-prepTime",
                "categoryName",
                "-categoryName"
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

    private static async Task<IResult> SearchRecipesAsync(
        HttpRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = request.Query;
        var searchTerm = query["q"].FirstOrDefault();

        if (!TryParsePositiveInt(query["page"], 1, out var page))
        {
            throw new ValidationException("page must be a positive integer.");
        }

        if (!TryParsePositiveInt(query["pageSize"], 10, out var pageSize)
            || pageSize > 50)
        {
            throw new ValidationException("pageSize must be between 1 and 50.");
        }

        var sort = query["sort"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(sort))
        {
            var sortBy = query["sortBy"].FirstOrDefault();
            var sortOrder = query["sortOrder"].FirstOrDefault() ?? "desc";
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["name"] = "title",
                    ["title"] = "title",
                    ["createdAt"] = "createdAt",
                    ["cookTime"] = "cookTime",
                    ["cookTimeMinutes"] = "cookTime",
                    ["prepTime"] = "prepTime",
                    ["prepTimeMinutes"] = "prepTime",
                    ["categoryName"] = "categoryName"
                };
                if (!fields.TryGetValue(sortBy, out var field)
                    || !string.Equals(sortOrder, "asc", StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ValidationException("sortBy or sortOrder is invalid.");
                }

                sort = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase)
                    ? $"-{field}"
                    : field;
            }
            else
            {
                sort = "-createdAt";
            }
        }

        var allowedSorts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "createdAt", "-createdAt", "title", "-title", "cookTime", "-cookTime",
            "prepTime", "-prepTime", "categoryName", "-categoryName"
        };
        if (!allowedSorts.Contains(sort))
        {
            throw new ValidationException("sort is invalid.");
        }

        var result = await sender.Send(
            new SearchRecipesQuery(searchTerm ?? string.Empty, page, pageSize, sort),
            cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> CreateRecipeAsync(
        CreateRecipeRequest request,
        RecipeManagementService service,
        IOutputCacheStore outputCache,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        await InvalidateRecipeCacheAsync(outputCache, cancellationToken);
        return Results.Created($"/api/v1/recipes/{result.Slug}", result);
    }

    private static async Task<IResult> UpdateRecipeAsync(
        Guid id,
        UpdateRecipeRequest request,
        RecipeManagementService service,
        IOutputCacheStore outputCache,
        CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(id, request, cancellationToken);
        await InvalidateRecipeCacheAsync(outputCache, cancellationToken);
        return Results.Ok(result);
    }

    private static Task<IResult> PublishRecipeAsync(
        Guid id,
        RecipeManagementService service,
        IOutputCacheStore outputCache,
        CancellationToken cancellationToken) =>
        SetRecipeStatusAsync(id, RecipeStatus.Published, service, outputCache, cancellationToken);

    private static Task<IResult> UnpublishRecipeAsync(
        Guid id,
        RecipeManagementService service,
        IOutputCacheStore outputCache,
        CancellationToken cancellationToken) =>
        SetRecipeStatusAsync(id, RecipeStatus.Draft, service, outputCache, cancellationToken);

    private static Task<IResult> ArchiveRecipeAsync(
        Guid id,
        RecipeManagementService service,
        IOutputCacheStore outputCache,
        CancellationToken cancellationToken) =>
        SetRecipeStatusAsync(id, RecipeStatus.Archived, service, outputCache, cancellationToken);

    private static async Task<IResult> SetRecipeStatusAsync(
        Guid id,
        RecipeStatus status,
        RecipeManagementService service,
        IOutputCacheStore outputCache,
        CancellationToken cancellationToken)
    {
        var result = await service.SetStatusAsync(id, status, cancellationToken);
        await InvalidateRecipeCacheAsync(outputCache, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> DeleteRecipeAsync(
        Guid id,
        RecipeManagementService service,
        IOutputCacheStore outputCache,
        CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        await InvalidateRecipeCacheAsync(outputCache, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> AddIngredientAsync(
        Guid id,
        RecipeIngredientInput request,
        RecipeManagementService service,
        IOutputCacheStore outputCache,
        CancellationToken cancellationToken)
    {
        var result = await service.AddIngredientAsync(id, request, cancellationToken);
        await InvalidateRecipeCacheAsync(outputCache, cancellationToken);
        return Results.Created($"/api/v1/recipes/{id}/ingredients/{result.Id}", result);
    }

    private static async Task<IResult> UpdateIngredientAsync(
        Guid id,
        Guid ingredientId,
        RecipeIngredientInput request,
        RecipeManagementService service,
        IOutputCacheStore outputCache,
        CancellationToken cancellationToken)
    {
        var result = await service.UpdateIngredientAsync(
            id, ingredientId, request, cancellationToken);
        await InvalidateRecipeCacheAsync(outputCache, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> DeleteIngredientAsync(
        Guid id,
        Guid ingredientId,
        RecipeManagementService service,
        IOutputCacheStore outputCache,
        CancellationToken cancellationToken)
    {
        await service.DeleteIngredientAsync(id, ingredientId, cancellationToken);
        await InvalidateRecipeCacheAsync(outputCache, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> AddStepAsync(
        Guid id,
        RecipeStepInput request,
        RecipeManagementService service,
        IOutputCacheStore outputCache,
        CancellationToken cancellationToken)
    {
        var result = await service.AddStepAsync(id, request, cancellationToken);
        await InvalidateRecipeCacheAsync(outputCache, cancellationToken);
        return Results.Created($"/api/v1/recipes/{id}/steps/{result.Id}", result);
    }

    private static async Task<IResult> UpdateStepAsync(
        Guid id,
        Guid stepId,
        RecipeStepInput request,
        RecipeManagementService service,
        IOutputCacheStore outputCache,
        CancellationToken cancellationToken)
    {
        var result = await service.UpdateStepAsync(id, stepId, request, cancellationToken);
        await InvalidateRecipeCacheAsync(outputCache, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> DeleteStepAsync(
        Guid id,
        Guid stepId,
        RecipeManagementService service,
        IOutputCacheStore outputCache,
        CancellationToken cancellationToken)
    {
        await service.DeleteStepAsync(id, stepId, cancellationToken);
        await InvalidateRecipeCacheAsync(outputCache, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> UploadImageAsync(
        Guid id,
        HttpRequest request,
        RecipeManagementService service,
        IOutputCacheStore outputCache,
        IConfiguration configuration,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("CulinaryBlog.FileUpload");
        var correlationId = CorrelationIdMiddleware.GetCorrelationId(request.HttpContext);
        string? fileName = null;
        string? contentType = null;
        string? objectKey = null;
        long? fileSize = null;

        try
        {
            if (!request.HasFormContentType)
            {
                throw new ValidationException("Content-Type must be multipart/form-data.");
            }

            var form = await request.ReadFormAsync(cancellationToken);
            var file = form.Files.GetFile("file");
            if (file is null || file.Length == 0)
            {
                throw new ValidationException("The file field is required and cannot be empty.");
            }

            fileName = Path.GetFileName(file.FileName.Replace('\\', '/'));
            fileSize = file.Length;
            contentType = file.ContentType;

            var maxFileSize = configuration.GetValue<long>(
                "FileUpload:MaxFileSizeBytes",
                5 * 1024 * 1024);
            await using var stream = file.OpenReadStream();
            var signature = new byte[12];
            var signatureLength = 0;
            while (signatureLength < signature.Length)
            {
                var bytesRead = await stream.ReadAsync(
                    signature.AsMemory(signatureLength),
                    cancellationToken);
                if (bytesRead == 0) break;
                signatureLength += bytesRead;
            }

            var extension = RecipeImageUploadValidator.Validate(
                fileName,
                file.ContentType,
                file.Length,
                maxFileSize,
                signature.AsSpan(0, signatureLength));

            stream.Position = 0;
            objectKey = $"recipes/{id}/{Guid.NewGuid():N}{extension}";
            var isPrimary = bool.TryParse(form["isPrimary"], out var primary) && primary;
            var image = await service.UploadImageAsync(
                id,
                stream,
                objectKey,
                file.Length,
                file.ContentType,
                form["altText"].FirstOrDefault(),
                isPrimary,
                cancellationToken);

            logger.LogInformation(
                "File upload completed {EventType} {CorrelationId} {FileName} {FileSize} {ContentType} {ObjectKey} {Status}",
                "FileUpload",
                correlationId,
                fileName,
                fileSize,
                contentType,
                objectKey,
                "Success");

            await InvalidateRecipeCacheAsync(outputCache, cancellationToken);
            return Results.Created($"/api/v1/recipes/{id}/images/{image.Id}", image);
        }
        catch (ValidationException exception)
        {
            logger.LogWarning(
                "File upload rejected {EventType} {CorrelationId} {FileName} {FileSize} {ContentType} {ObjectKey} {Status} {ErrorType} {Reason}",
                "FileUpload",
                correlationId,
                fileName,
                fileSize,
                contentType,
                objectKey,
                "Failed",
                exception.GetType().Name,
                exception.Message);
            throw;
        }
        catch (StorageUnavailableException exception)
        {
            logger.LogError(
                exception,
                "File upload failed {EventType} {CorrelationId} {FileName} {FileSize} {ContentType} {ObjectKey} {Status} {ErrorType} {Reason}",
                "FileUpload",
                correlationId,
                fileName,
                fileSize,
                contentType,
                objectKey,
                "Failed",
                exception.GetType().Name,
                "StorageUnavailable");
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "File upload failed {EventType} {CorrelationId} {FileName} {FileSize} {ContentType} {ObjectKey} {Status} {ErrorType} {Reason}",
                "FileUpload",
                correlationId,
                fileName,
                fileSize,
                contentType,
                objectKey,
                "Failed",
                exception.GetType().Name,
                "UnexpectedFailure");
            throw;
        }
    }

    private static async Task<IResult> UpdateImageMetadataAsync(
        Guid id,
        Guid imageId,
        RecipeImageMetadataInput request,
        RecipeManagementService service,
        IOutputCacheStore outputCache,
        CancellationToken cancellationToken)
    {
        var image = await service.UpdateImageMetadataAsync(
            id, imageId, request, cancellationToken);
        await InvalidateRecipeCacheAsync(outputCache, cancellationToken);
        return Results.Ok(image);
    }

    private static async Task<IResult> SetPrimaryImageAsync(
        Guid id,
        Guid imageId,
        RecipeManagementService service,
        IOutputCacheStore outputCache,
        CancellationToken cancellationToken)
    {
        var image = await service.SetPrimaryImageAsync(id, imageId, cancellationToken);
        await InvalidateRecipeCacheAsync(outputCache, cancellationToken);
        return Results.Ok(image);
    }

    private static async Task<IResult> DeleteImageAsync(
        Guid id,
        Guid imageId,
        RecipeManagementService service,
        IOutputCacheStore outputCache,
        CancellationToken cancellationToken)
    {
        await service.DeleteImageAsync(id, imageId, cancellationToken);
        await InvalidateRecipeCacheAsync(outputCache, cancellationToken);
        return Results.NoContent();
    }

    private static async Task InvalidateRecipeCacheAsync(
        IOutputCacheStore outputCache,
        CancellationToken cancellationToken)
    {
        await outputCache.EvictByTagAsync("recipes", cancellationToken);
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
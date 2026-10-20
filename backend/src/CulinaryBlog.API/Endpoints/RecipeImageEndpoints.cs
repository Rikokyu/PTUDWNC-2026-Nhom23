using CulinaryBlog.Application.Features.Recipes.Commands;
using Asp.Versioning;
using Asp.Versioning.Builder;
using MediatR;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Recipes.Commands.UploadRecipeImage;

namespace CulinaryBlog.API.Endpoints;

public static class RecipeImageEndpoints
{
	public static IEndpointRouteBuilder MapRecipeImageEndpoints(
		this IEndpointRouteBuilder endpoints,
		ApiVersionSet versionSet)
	{
		var group = endpoints
			.MapGroup("/api/v1/recipes/{recipeId:guid}/images")
			.WithApiVersionSet(versionSet)
			.MapToApiVersion(new ApiVersion(1, 0))
			.WithTags("Recipe Images")
			.RequireRateLimiting("Api");

		group.MapPost(
			"",
			async (
				Guid recipeId,
				HttpRequest httpRequest,
				IConfiguration configuration,
				ISender sender,
				CancellationToken cancellationToken) =>
			{
				if (httpRequest.HasFormContentType)
				{
					var form = await httpRequest.ReadFormAsync(cancellationToken);
					var file = form.Files.GetFile("file")
						?? throw new ValidationException("file is required.");
					var limit = configuration.GetValue<long>("FileUpload:MaxFileSizeBytes", 5 * 1024 * 1024);
					if (file.Length <= 0 || file.Length > limit)
						throw new ValidationException("Image size exceeds the configured limit or is empty.");
					using var buffer = new MemoryStream();
					await file.CopyToAsync(buffer, cancellationToken);
					var content = buffer.ToArray();
					RecipeImageUploadValidator.Validate(file.FileName, file.ContentType, file.Length, limit, content);
					bool? isPrimary = null;
					if (form.TryGetValue("isPrimary", out var primary) && !string.IsNullOrWhiteSpace(primary))
					{
						if (!bool.TryParse(primary, out var parsed))
							throw new ValidationException("isPrimary must be a boolean.");
						isPrimary = parsed;
					}
					var image = await sender.Send(new UploadRecipeImageCommand(
						recipeId, content, file.ContentType, form["altText"].FirstOrDefault(), isPrimary), cancellationToken);
					return Results.Created($"/api/v1/recipes/{recipeId}/images/{image.Id}", image);
				}
				var request = await httpRequest.ReadFromJsonAsync<RecipeImageRequest>(cancellationToken)
					?? throw new ValidationException("An image request is required.");
				var id = await sender.Send(
					new AddRecipeImageCommand(
						recipeId,
						request.OriginalUrl,
						request.AltText,
						request.IsPrimary),
					cancellationToken);
				return Results.Created(
					$"/api/v1/recipes/{recipeId}/images/{id}",
					new { id });
			})
			.RequireRateLimiting("Upload")
			.RequireAuthorization();

		group.MapPatch(
			"/{imageId:guid}",
			async (
				Guid recipeId,
				Guid imageId,
				RecipeImageUpdateRequest request,
				ISender sender,
				CancellationToken cancellationToken) =>
			{
				await sender.Send(
					new UpdateRecipeImageCommand(
						recipeId,
						imageId,
						request.AltText,
						request.IsPrimary),
					cancellationToken);
				return Results.NoContent();
			})
			.WithSummary("Update a recipe image.")
			.RequireAuthorization();

		group.MapDelete(
			"/{imageId:guid}",
			async (
				Guid recipeId,
				Guid imageId,
				ISender sender,
				CancellationToken cancellationToken) =>
			{
				await sender.Send(
					new DeleteRecipeImageCommand(recipeId, imageId),
					cancellationToken);
				return Results.NoContent();
			})
			.WithSummary("Delete a recipe image.")
			.RequireAuthorization();

		return endpoints;
	}
}

public sealed record RecipeImageRequest(
	string OriginalUrl,
	string? AltText,
	bool IsPrimary = false);

public sealed record RecipeImageUpdateRequest(
	string? AltText,
	bool? IsPrimary);

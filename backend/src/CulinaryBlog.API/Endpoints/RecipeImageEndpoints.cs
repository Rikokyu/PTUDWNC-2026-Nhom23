using CulinaryBlog.Application.Features.Recipes.Commands;
using MediatR;

namespace CulinaryBlog.API.Endpoints;

public static class RecipeImageEndpoints
{
	public static IEndpointRouteBuilder MapRecipeImageEndpoints(
		this IEndpointRouteBuilder endpoints)
	{
		var group = endpoints
			.MapGroup("/api/v1/recipes/{recipeId:guid}/images")
			.WithTags("Recipe Images");

		group.MapPost(
			"",
			async (
				Guid recipeId,
				RecipeImageRequest request,
				ISender sender,
				CancellationToken cancellationToken) =>
			{
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
			.WithSummary("Add an image to a recipe.")
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

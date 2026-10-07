using CulinaryBlog.Application.Features.Recipes.Commands;
using MediatR;

namespace CulinaryBlog.API.Endpoints;

public static class RecipeIngredientEndpoints
{
	public static IEndpointRouteBuilder MapRecipeIngredientEndpoints(
		this IEndpointRouteBuilder endpoints)
	{
		var group = endpoints
			.MapGroup("/api/v1/recipes/{recipeId:guid}/ingredients")
			.WithTags("Recipe Ingredients");

		group.MapPost(
			"",
			async (
				Guid recipeId,
				RecipeIngredientRequest request,
				ISender sender,
				CancellationToken cancellationToken) =>
			{
				var id = await sender.Send(
					new AddRecipeIngredientCommand(
						recipeId,
						request.Name,
						request.Quantity,
						request.Unit,
						request.Notes),
					cancellationToken);
				return Results.Created(
					$"/api/v1/recipes/{recipeId}/ingredients/{id}",
					new { id });
			});

		group.MapPatch(
			"/{ingredientId:guid}",
			async (
				Guid recipeId,
				Guid ingredientId,
				RecipeIngredientRequest request,
				ISender sender,
				CancellationToken cancellationToken) =>
			{
				await sender.Send(
					new UpdateRecipeIngredientCommand(
						recipeId,
						ingredientId,
						request.Name,
						request.Quantity,
						request.Unit,
						request.Notes),
					cancellationToken);
				return Results.NoContent();
			});

		group.MapDelete(
			"/{ingredientId:guid}",
			async (
				Guid recipeId,
				Guid ingredientId,
				ISender sender,
				CancellationToken cancellationToken) =>
			{
				await sender.Send(
					new DeleteRecipeIngredientCommand(recipeId, ingredientId),
					cancellationToken);
				return Results.NoContent();
			});

		return endpoints;
	}
}

public sealed record RecipeIngredientRequest(
	string Name,
	string? Quantity,
	string? Unit,
	string? Notes);

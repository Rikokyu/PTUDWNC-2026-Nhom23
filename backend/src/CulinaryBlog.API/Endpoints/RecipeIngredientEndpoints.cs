using CulinaryBlog.Application.Features.Recipes.Commands;
using Asp.Versioning;
using Asp.Versioning.Builder;
using MediatR;
using Microsoft.AspNetCore.OutputCaching;

namespace CulinaryBlog.API.Endpoints;

public static class RecipeIngredientEndpoints
{
	public static IEndpointRouteBuilder MapRecipeIngredientEndpoints(
		this IEndpointRouteBuilder endpoints,
		ApiVersionSet versionSet)
	{
		var group = endpoints
			.MapGroup("/api/v1/recipes/{recipeId:guid}/ingredients")
			.WithApiVersionSet(versionSet)
			.MapToApiVersion(new ApiVersion(1, 0))
			.WithTags("Recipe Ingredients")
			.RequireRateLimiting("Api");

		group.MapPost(
			"",
			async (
				Guid recipeId,
				RecipeIngredientRequest request,
				ISender sender,
				IOutputCacheStore outputCacheStore,
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
				await outputCacheStore.EvictByTagAsync(
					"recipes",
					cancellationToken);
				return Results.Created(
					$"/api/v1/recipes/{recipeId}/ingredients/{id}",
					new { id });
			})
			.WithSummary("Add an ingredient to a recipe.")
			.RequireAuthorization();

		group.MapPut(
			"/{ingredientId:guid}",
			async (
				Guid recipeId,
				Guid ingredientId,
				RecipeIngredientRequest request,
				ISender sender,
				IOutputCacheStore outputCacheStore,
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
				await outputCacheStore.EvictByTagAsync(
					"recipes",
					cancellationToken);
				return Results.Ok();
			})
			.WithSummary("Update a recipe ingredient.")
			.Produces(StatusCodes.Status200OK)
			.RequireAuthorization();

		group.MapDelete(
			"/{ingredientId:guid}",
			async (
				Guid recipeId,
				Guid ingredientId,
				ISender sender,
				IOutputCacheStore outputCacheStore,
				CancellationToken cancellationToken) =>
			{
				await sender.Send(
					new DeleteRecipeIngredientCommand(recipeId, ingredientId),
					cancellationToken);
				await outputCacheStore.EvictByTagAsync(
					"recipes",
					cancellationToken);
				return Results.NoContent();
			})
			.WithSummary("Delete a recipe ingredient.")
			.RequireAuthorization();

		return endpoints;
	}
}

public sealed record RecipeIngredientRequest(
	string Name,
	string? Quantity,
	string? Unit,
	string? Notes);

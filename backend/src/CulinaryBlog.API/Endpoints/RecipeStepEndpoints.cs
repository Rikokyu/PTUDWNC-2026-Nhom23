using CulinaryBlog.Application.Features.Recipes.Commands;
using Asp.Versioning;
using Asp.Versioning.Builder;
using MediatR;
using Microsoft.AspNetCore.OutputCaching;

namespace CulinaryBlog.API.Endpoints;

public static class RecipeStepEndpoints
{
	public static IEndpointRouteBuilder MapRecipeStepEndpoints(
		this IEndpointRouteBuilder endpoints,
		ApiVersionSet versionSet)
	{
		var group = endpoints
			.MapGroup("/api/v1/recipes/{recipeId:guid}/steps")
			.WithApiVersionSet(versionSet)
			.MapToApiVersion(new ApiVersion(1, 0))
			.WithTags("Recipe Steps")
			.RequireRateLimiting("Api");

		group.MapPost(
			"",
			async (
				Guid recipeId,
				RecipeStepRequest request,
				ISender sender,
				IOutputCacheStore outputCacheStore,
				CancellationToken cancellationToken) =>
			{
				var id = await sender.Send(
					new AddRecipeStepCommand(
						recipeId,
						request.Title,
						request.Description,
						request.TimerMinutes,
						request.ImageUrl),
					cancellationToken);
				await outputCacheStore.EvictByTagAsync(
					"recipes",
					cancellationToken);
				return Results.Created(
					$"/api/v1/recipes/{recipeId}/steps/{id}",
					new { id });
			})
			.WithSummary("Add a preparation step to a recipe.")
			.RequireAuthorization("AuthorOrAdmin");

		group.MapPut(
			"/{stepId:guid}",
			async (
				Guid recipeId,
				Guid stepId,
				RecipeStepRequest request,
				ISender sender,
				IOutputCacheStore outputCacheStore,
				CancellationToken cancellationToken) =>
			{
				await sender.Send(
					new UpdateRecipeStepCommand(
						recipeId,
						stepId,
						request.Title,
						request.Description,
						request.TimerMinutes,
						request.ImageUrl),
					cancellationToken);
				await outputCacheStore.EvictByTagAsync(
					"recipes",
					cancellationToken);
				return Results.Ok();
			})
			.WithSummary("Update a recipe preparation step.")
			.Produces(StatusCodes.Status200OK)
			.RequireAuthorization("AuthorOrAdmin");

		group.MapDelete(
			"/{stepId:guid}",
			async (
				Guid recipeId,
				Guid stepId,
				ISender sender,
				IOutputCacheStore outputCacheStore,
				CancellationToken cancellationToken) =>
			{
				await sender.Send(
					new DeleteRecipeStepCommand(recipeId, stepId),
					cancellationToken);
				await outputCacheStore.EvictByTagAsync(
					"recipes",
					cancellationToken);
				return Results.NoContent();
			})
			.WithSummary("Delete a recipe preparation step.")
			.RequireAuthorization("AuthorOrAdmin");

		group.MapPut(
			"/order",
			async (
				Guid recipeId,
				ReorderRecipeStepsRequest request,
				ISender sender,
				IOutputCacheStore outputCacheStore,
				CancellationToken cancellationToken) =>
			{
				await sender.Send(
					new ReorderRecipeStepsCommand(recipeId, request.StepIds),
					cancellationToken);
				await outputCacheStore.EvictByTagAsync(
					"recipes",
					cancellationToken);
				return Results.Ok();
			})
			.WithSummary("Reorder recipe steps and assign consecutive step numbers.")
			.RequireAuthorization("AuthorOrAdmin");

		return endpoints;
	}
}

public sealed record RecipeStepRequest(
	string Title,
	string Description,
	int? TimerMinutes,
	string? ImageUrl);

public sealed record ReorderRecipeStepsRequest(
	IReadOnlyList<Guid> StepIds);

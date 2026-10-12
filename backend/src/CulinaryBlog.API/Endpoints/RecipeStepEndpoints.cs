using CulinaryBlog.Application.Features.Recipes.Commands;
using MediatR;

namespace CulinaryBlog.API.Endpoints;

public static class RecipeStepEndpoints
{
	public static IEndpointRouteBuilder MapRecipeStepEndpoints(
		this IEndpointRouteBuilder endpoints)
	{
		var group = endpoints
			.MapGroup("/api/v1/recipes/{recipeId:guid}/steps")
			.WithTags("Recipe Steps");

		group.MapPost(
			"",
			async (
				Guid recipeId,
				RecipeStepRequest request,
				ISender sender,
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
				return Results.Created(
					$"/api/v1/recipes/{recipeId}/steps/{id}",
					new { id });
			});

		group.MapPatch(
			"/{stepId:guid}",
			async (
				Guid recipeId,
				Guid stepId,
				RecipeStepRequest request,
				ISender sender,
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
				return Results.NoContent();
			});

		group.MapDelete(
			"/{stepId:guid}",
			async (
				Guid recipeId,
				Guid stepId,
				ISender sender,
				CancellationToken cancellationToken) =>
			{
				await sender.Send(
					new DeleteRecipeStepCommand(recipeId, stepId),
					cancellationToken);
				return Results.NoContent();
			});

		return endpoints;
	}
}

public sealed record RecipeStepRequest(
	string Title,
	string Description,
	int? TimerMinutes,
	string? ImageUrl);

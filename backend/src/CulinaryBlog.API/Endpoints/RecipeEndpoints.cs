using CulinaryBlog.Application.Features.Recipes.Commands.CreateRecipe;
using CulinaryBlog.Application.Features.Recipes.Commands.DeleteRecipe;
using CulinaryBlog.Application.Features.Recipes.Commands.UpdateRecipe;
using CulinaryBlog.Application.Features.Recipes.Queries.GetRecipeById;
using CulinaryBlog.Application.Features.Recipes.Queries.GetRecipes;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CulinaryBlog.API.Endpoints;

public static class RecipeEndpoints
{
	public static IEndpointRouteBuilder MapRecipeEndpoints(this IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/recipes").WithTags("Recipes");

		group.MapGet("/", async (ISender sender, CancellationToken cancellationToken) =>
		{
			var recipes = await sender.Send(new GetRecipesQuery(), cancellationToken);
			return Results.Ok(recipes);
		});

		group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
		{
			var recipe = await sender.Send(new GetRecipeByIdQuery(id), cancellationToken);
			return recipe is null ? Results.NotFound() : Results.Ok(recipe);
		});

		group.MapPost("/", async (CreateRecipeRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Description))
			{
				return Results.BadRequest(new { error = "Title and description are required." });
			}

			var command = new CreateRecipeCommand(request.Title, request.Description, request.CategoryId);
			var recipe = await sender.Send(command, cancellationToken);
			return recipe is null
				? Results.BadRequest(new { error = "The specified category does not exist." })
				: Results.Created($"/api/recipes/{recipe.Id}", recipe);
		})
		.RequireAuthorization();

		group.MapPut("/{id:guid}", async (Guid id, UpdateRecipeRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Description))
			{
				return Results.BadRequest(new { error = "Title and description are required." });
			}

			var command = new UpdateRecipeCommand(id, request.Title, request.Description, request.CategoryId);
			var recipe = await sender.Send(command, cancellationToken);
			return recipe is null ? Results.NotFound() : Results.Ok(recipe);
		})
		.RequireAuthorization();

		group.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
		{
			var deleted = await sender.Send(new DeleteRecipeCommand(id), cancellationToken);
			return deleted ? Results.NoContent() : Results.NotFound();
		})
		.RequireAuthorization();

		return app;
	}
}

public sealed record CreateRecipeRequest(string Title, string Description, Guid CategoryId);

public sealed record UpdateRecipeRequest(string Title, string Description, Guid CategoryId);

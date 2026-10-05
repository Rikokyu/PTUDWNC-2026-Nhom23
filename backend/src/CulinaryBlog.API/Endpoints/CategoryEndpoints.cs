using CulinaryBlog.Application.DTOs.Categories;
using CulinaryBlog.Application.Features.Categories.Commands.CreateCategory;
using CulinaryBlog.Application.Features.Categories.Commands.DeleteCategory;
using CulinaryBlog.Application.Features.Categories.Commands.UpdateCategory;
using CulinaryBlog.Application.Features.Categories.Queries.GetCategories;
using CulinaryBlog.Application.Features.Categories.Queries.GetCategoryById;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CulinaryBlog.API.Endpoints;

public static class CategoryEndpoints
{
	public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder app)
	{
		var group = app.MapGroup("/api/categories").WithTags("Categories");

		group.MapGet("/", async (ISender sender, CancellationToken cancellationToken) =>
		{
			var categories = await sender.Send(new GetCategoriesQuery(), cancellationToken);
			return Results.Ok(categories);
		});

		group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
		{
			var category = await sender.Send(new GetCategoryByIdQuery(id), cancellationToken);
			return category is null ? Results.NotFound() : Results.Ok(category);
		});

		group.MapPost("/", async (CreateCategoryCommand command, ISender sender, CancellationToken cancellationToken) =>
		{
			if (string.IsNullOrWhiteSpace(command.Name))
			{
				return Results.BadRequest(new { error = "Category name is required." });
			}

			var category = await sender.Send(command, cancellationToken);
			return Results.Created($"/api/categories/{category.Id}", category);
		})
		.RequireAuthorization();

		group.MapPut("/{id:guid}", async (Guid id, UpdateCategoryRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			if (string.IsNullOrWhiteSpace(request.Name))
			{
				return Results.BadRequest(new { error = "Category name is required." });
			}

			var command = new UpdateCategoryCommand(id, request.Name, request.Description);
			var category = await sender.Send(command, cancellationToken);
			return category is null ? Results.NotFound() : Results.Ok(category);
		})
		.RequireAuthorization();

		group.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
		{
			var deleted = await sender.Send(new DeleteCategoryCommand(id), cancellationToken);
			return deleted ? Results.NoContent() : Results.NotFound();
		})
		.RequireAuthorization();

		return app;
	}
}

public sealed record UpdateCategoryRequest(string Name, string? Description);

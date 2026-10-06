using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.DTOs.Categories;
using CulinaryBlog.Application.Features.Categories.Commands.CreateCategory;
using CulinaryBlog.Application.Features.Categories.Commands.DeleteCategory;
using CulinaryBlog.Application.Features.Categories.Commands.UpdateCategory;
using CulinaryBlog.Application.Features.Categories.Queries.GetCategories;
using CulinaryBlog.Application.Features.Categories.Queries.GetCategoryBySlug;
using MediatR;

namespace CulinaryBlog.API.Endpoints;

public static class CategoryEndpoints
{
	public static IEndpointRouteBuilder MapCategoryEndpoints(
		this IEndpointRouteBuilder endpoints)
	{
		var group = endpoints
			.MapGroup("/api/v1/categories")
			.WithTags("Categories");

		group.MapGet("", GetCategoriesAsync)
			.WithName("GetCategories")
			.Produces<IReadOnlyList<CategoryDto>>();
		group.MapGet("/{slug}", GetCategoryBySlugAsync)
			.WithName("GetCategoryBySlug")
			.Produces<CategoryDetailDto>()
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status404NotFound);
		group.MapPost("", CreateCategoryAsync)
			.RequireAuthorization(policy => policy.RequireRole("Admin"))
			.WithName("CreateCategory")
			.Produces<CategoryDto>(StatusCodes.Status201Created)
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status409Conflict);
		group.MapPut("/{id:guid}", UpdateCategoryAsync)
			.RequireAuthorization(policy => policy.RequireRole("Admin"))
			.WithName("UpdateCategory")
			.Produces<CategoryDto>()
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.ProducesProblem(StatusCodes.Status409Conflict);
		group.MapDelete("/{id:guid}", DeleteCategoryAsync)
			.RequireAuthorization(policy => policy.RequireRole("Admin"))
			.WithName("DeleteCategory")
			.Produces(StatusCodes.Status204NoContent)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.ProducesProblem(StatusCodes.Status409Conflict);

		return endpoints;
	}

	private static async Task<IResult> GetCategoriesAsync(
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new GetCategoriesQuery(),
			cancellationToken);

		return Results.Ok(result);
	}

	private static async Task<IResult> GetCategoryBySlugAsync(
		string slug,
		HttpRequest request,
		ISender sender,
		CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(slug))
		{
			throw new ValidationException("slug is required.");
		}

		var page = ParsePositiveInt(request.Query["page"], 1, "page");
		var pageSize = ParsePositiveInt(request.Query["pageSize"], 12, "pageSize");
		if (pageSize > 50)
		{
			throw new ValidationException("pageSize cannot be greater than 50.");
		}

		var result = await sender.Send(
			new GetCategoryBySlugQuery(slug, page, pageSize),
			cancellationToken);

		return Results.Ok(result);
	}

	private static int ParsePositiveInt(
		string? value,
		int defaultValue,
		string parameterName)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return defaultValue;
		}

		if (!int.TryParse(value, out var parsed) || parsed < 1)
		{
			throw new ValidationException($"{parameterName} must be a positive integer.");
		}

		return parsed;
	}

	private static async Task<IResult> CreateCategoryAsync(
		CreateCategoryRequest request,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new CreateCategoryCommand(
				request.Name ?? string.Empty,
				request.Description,
				request.ImageUrl,
				request.OrderIndex),
			cancellationToken);

		return Results.Created($"/api/v1/categories/{result.Slug}", result);
	}

	private static async Task<IResult> UpdateCategoryAsync(
		Guid id,
		UpdateCategoryRequest request,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new UpdateCategoryCommand(
				id,
				request.Name ?? string.Empty,
				request.Description,
				request.ImageUrl,
				request.OrderIndex),
			cancellationToken);

		return Results.Ok(result);
	}

	private static async Task<IResult> DeleteCategoryAsync(
		Guid id,
		ISender sender,
		CancellationToken cancellationToken)
	{
		await sender.Send(new DeleteCategoryCommand(id), cancellationToken);
		return Results.NoContent();
	}

	public sealed record CreateCategoryRequest(
		string? Name,
		string? Description,
		string? ImageUrl,
		int OrderIndex = 0);

	public sealed record UpdateCategoryRequest(
		string? Name,
		string? Description,
		string? ImageUrl,
		int OrderIndex = 0);
}

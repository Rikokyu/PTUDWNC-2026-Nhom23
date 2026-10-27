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
			.WithTags("Categories")
			.RequireRateLimiting("Api");

		group.MapGet("", GetCategoriesAsync)
			.WithName("GetCategories")
			.WithSummary("List active recipe categories.")
			.Produces<IReadOnlyList<CategoryDto>>(StatusCodes.Status200OK);

		group.MapGet("/{slug}", GetCategoryBySlugAsync)
			.WithName("GetCategoryBySlug")
			.WithSummary("Get a category and its paginated recipes.")
			.Produces<CategoryDetailDto>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.ProducesProblem(StatusCodes.Status422UnprocessableEntity);

		group.MapPost("", CreateCategoryAsync)
			.WithName("CreateCategory")
			.WithSummary("Create a recipe category.")
			.RequireAuthorization("Admin")
			.Produces<CategoryDto>(StatusCodes.Status201Created)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status409Conflict)
			.ProducesProblem(StatusCodes.Status422UnprocessableEntity);

		group.MapPut("/{id:guid}", UpdateCategoryAsync)
			.WithName("UpdateCategory")
			.WithSummary("Update a recipe category.")
			.RequireAuthorization("Admin")
			.Produces<CategoryDto>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.ProducesProblem(StatusCodes.Status409Conflict)
			.ProducesProblem(StatusCodes.Status422UnprocessableEntity);

		group.MapDelete("/{id:guid}", DeleteCategoryAsync)
			.WithName("DeleteCategory")
			.WithSummary("Soft-delete a recipe category.")
			.RequireAuthorization("Admin")
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

		// Parse here so malformed query values return 400 without a binding exception.
		if (!TryParsePagination(request.Query, "page", 1, out var page))
		{
			return Results.Problem(statusCode: StatusCodes.Status400BadRequest,
				detail: "page must be a positive integer.");
		}

		if (!TryParsePagination(request.Query, "pageSize", 12, out var pageSize)
			|| pageSize > 50 || (long)(page - 1) * pageSize > int.MaxValue)
		{
			return Results.Problem(statusCode: StatusCodes.Status400BadRequest,
				detail: "pageSize must be between 1 and 50 and pagination must not overflow.");
		}

		var result = await sender.Send(
			new GetCategoryBySlugQuery(
				slug.Trim(),
				page,
				pageSize),
			cancellationToken);

		return Results.Ok(result);
	}

	private static bool TryParsePagination(IQueryCollection query, string key, int defaultValue, out int value)
	{
		value = defaultValue;
		if (!query.TryGetValue(key, out var values)) return true;
		return values.Count == 1 && int.TryParse(values[0], out value) && value >= 1;
	}

	private static async Task<IResult> CreateCategoryAsync(
		CreateCategoryRequest request,
		ISender sender,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new CreateCategoryCommand(
				request.Name,
				request.Description,
				request.ImageUrl,
				request.OrderIndex),
			cancellationToken);

		return Results.Created(
			$"/api/v1/categories/{result.Slug}",
			result);
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
				request.Name,
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
		await sender.Send(
			new DeleteCategoryCommand(id),
			cancellationToken);

		return Results.NoContent();
	}
}

public sealed record CreateCategoryRequest(
	string Name,
	string? Description,
	string? ImageUrl,
	int OrderIndex = 0);

public sealed record UpdateCategoryRequest(
	string? Name,
	string? Description,
	string? ImageUrl,
	int? OrderIndex);

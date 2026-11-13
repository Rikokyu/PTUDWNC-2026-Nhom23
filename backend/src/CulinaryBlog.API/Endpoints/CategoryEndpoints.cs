using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.DTOs.Categories;
using CulinaryBlog.Application.Features.Categories.Commands.CreateCategory;
using CulinaryBlog.Application.Features.Categories.Commands.DeleteCategory;
using CulinaryBlog.Application.Features.Categories.Commands.UpdateCategory;
using CulinaryBlog.Application.Features.Categories.Queries.GetCategories;
using CulinaryBlog.Application.Features.Categories.Queries.GetCategoryBySlug;
using MediatR;
using Microsoft.AspNetCore.OutputCaching;

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
			.CacheOutput("CategoryList")
			.WithName("GetCategories")
			.WithSummary("List active recipe categories.")
			.Produces<IReadOnlyList<CategoryDto>>(StatusCodes.Status200OK);

		group.MapGet("/{slug}", GetCategoryBySlugAsync)
			.CacheOutput("CategoryDetail")
			.WithName("GetCategoryBySlug")
			.WithSummary("Get a category and its paginated recipes.")
			.Produces<CategoryDetailDto>(StatusCodes.Status200OK)
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

		var page = ReadPaginationParameter(request, "page", 1);
		var pageSize = ReadPaginationParameter(request, "pageSize", 12);

		if (page < 1)
		{
			throw new ValidationException(
				"page must be a positive integer.");
		}

		if (pageSize is < 1 or > 50)
		{
			throw new ValidationException(
				"pageSize must be between 1 and 50.");
		}

		var result = await sender.Send(
			new GetCategoryBySlugQuery(
				slug.Trim(),
				page,
				pageSize),
			cancellationToken);

		return Results.Ok(result);
	}

	private static int ReadPaginationParameter(
		HttpRequest request,
		string name,
		int defaultValue)
	{
		if (!request.Query.TryGetValue(name, out var values))
		{
			return defaultValue;
		}

		if (values.Count != 1 || !int.TryParse(
			values[0],
			System.Globalization.NumberStyles.Integer,
			System.Globalization.CultureInfo.InvariantCulture,
			out var value))
		{
			throw new ValidationException($"{name} must be an integer.");
		}

		return value;
	}

	private static async Task<IResult> CreateCategoryAsync(
		CreateCategoryRequest request,
		ISender sender,
		IOutputCacheStore cache,
		CancellationToken cancellationToken)
	{
		var result = await sender.Send(
			new CreateCategoryCommand(
				request.Name,
				request.Description,
				request.ImageUrl,
				request.OrderIndex),
			cancellationToken);
		await cache.SafeEvictByTagAsync("categories", cancellationToken);

		return Results.Created(
			$"/api/v1/categories/{result.Slug}",
			result);
	}

	private static async Task<IResult> UpdateCategoryAsync(
		Guid id,
		UpdateCategoryRequest request,
		ISender sender,
		IOutputCacheStore cache,
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
		await cache.SafeEvictByTagAsync("categories", cancellationToken);

		return Results.Ok(result);
	}

	private static async Task<IResult> DeleteCategoryAsync(
		Guid id,
		ISender sender,
		IOutputCacheStore cache,
		CancellationToken cancellationToken)
	{
		await sender.Send(
			new DeleteCategoryCommand(id),
			cancellationToken);
		await cache.SafeEvictByTagAsync("categories", cancellationToken);

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

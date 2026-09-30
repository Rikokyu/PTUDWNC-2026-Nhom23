using CulinaryBlog.Application.Common.Exceptions;
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

		group.MapGet("", GetCategoriesAsync);
		group.MapGet("/{slug}", GetCategoryBySlugAsync);

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
		ISender sender,
		CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(slug))
		{
			throw new ValidationException("slug is required.");
		}

		var result = await sender.Send(
			new GetCategoryBySlugQuery(slug),
			cancellationToken);

		return Results.Ok(result);
	}
}

using CulinaryBlog.Application.DTOs.Categories;
using CulinaryBlog.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace CulinaryBlog.Application.Features.Categories.Queries.GetCategories;

public sealed class GetCategoriesQueryHandler
	: IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>
{
	private readonly IUnitOfWork _unitOfWork;
	public const string CacheKey = "categories:all";

	public GetCategoriesQueryHandler(
		IUnitOfWork unitOfWork,
		IMemoryCache cache)
	{
		_unitOfWork = unitOfWork;
		_ = cache; // Keep the existing constructor contract; HTTP output cache is shared in Redis.
	}

	public async Task<IReadOnlyList<CategoryDto>> Handle(
		GetCategoriesQuery request,
		CancellationToken cancellationToken)
	{
		var categories = await _unitOfWork.Categories
			.GetAllWithPublishedRecipeCountAsync(cancellationToken);

		var result = categories
			.Select(item => new CategoryDto(
				item.Category.Id,
				item.Category.Name,
				item.Category.Slug,
				item.Category.Description,
				string.IsNullOrWhiteSpace(item.Category.ImageUrl)
					? item.RepresentativeImageUrl
					: item.Category.ImageUrl,
				item.Category.OrderIndex,
				item.RecipeCount))
			.ToList();

		return result;
	}
}

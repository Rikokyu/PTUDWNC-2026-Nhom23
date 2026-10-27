using CulinaryBlog.Application.DTOs.Categories;
using CulinaryBlog.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace CulinaryBlog.Application.Features.Categories.Queries.GetCategories;

public sealed class GetCategoriesQueryHandler
	: IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>
{
	private readonly ICategoryRepository _categoryRepository;
	private readonly IMemoryCache _cache;

	public const string CacheKey = "categories:all";

	public GetCategoriesQueryHandler(
		ICategoryRepository categoryRepository,
		IMemoryCache cache)
	{
		_categoryRepository = categoryRepository;
		_cache = cache;
	}

	public async Task<IReadOnlyList<CategoryDto>> Handle(
		GetCategoriesQuery request,
		CancellationToken cancellationToken)
	{
		if (_cache.TryGetValue<IReadOnlyList<CategoryDto>>(
				CacheKey,
				out var cachedCategories)
			&& cachedCategories is not null)
		{
			return cachedCategories;
		}

		var categories = await _categoryRepository
			.GetAllWithPublishedRecipeCountAsync(cancellationToken);

		var result = categories
			.Select(item => new CategoryDto(
				item.Category.Id,
				item.Category.Name,
				item.Category.Slug,
				item.Category.Description,
				item.Category.ImageUrl,
				item.Category.OrderIndex,
				item.RecipeCount))
			.ToList();

		_cache.Set(
			CacheKey,
			result,
			new MemoryCacheEntryOptions
			{
				SlidingExpiration = TimeSpan.FromMinutes(60)
			});

		return result;
	}
}

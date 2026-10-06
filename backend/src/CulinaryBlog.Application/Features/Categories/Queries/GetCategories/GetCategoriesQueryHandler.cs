using CulinaryBlog.Application.DTOs.Categories;
using CulinaryBlog.Domain.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Features.Categories.Queries.GetCategories;

public sealed class GetCategoriesQueryHandler
	: IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>
{
	private readonly ICategoryRepository _categoryRepository;

	public GetCategoriesQueryHandler(ICategoryRepository categoryRepository)
	{
		_categoryRepository = categoryRepository;
	}

	public async Task<IReadOnlyList<CategoryDto>> Handle(
		GetCategoriesQuery request,
		CancellationToken cancellationToken)
	{
		var categories = await _categoryRepository.GetAllWithRecipeCountAsync(cancellationToken);

		return categories
			.Select(item => new CategoryDto(
				item.Category.Id,
				item.Category.Name,
				item.Category.Slug,
				item.Category.Description,
				item.Category.ImageUrl,
				item.Category.OrderIndex,
				item.RecipeCount))
			.ToList();
	}
}

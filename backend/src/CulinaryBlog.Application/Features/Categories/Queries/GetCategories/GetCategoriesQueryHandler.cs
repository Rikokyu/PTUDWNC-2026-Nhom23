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
		var categories = await _categoryRepository.GetAllAsync(cancellationToken);

		return categories
			.OrderBy(category => category.OrderIndex)
			.ThenBy(category => category.Name)
			.Select(category => new CategoryDto(
				category.Id,
				category.Name,
				category.Slug,
				category.Description,
				category.ImageUrl,
				category.OrderIndex))
			.ToList();
	}
}

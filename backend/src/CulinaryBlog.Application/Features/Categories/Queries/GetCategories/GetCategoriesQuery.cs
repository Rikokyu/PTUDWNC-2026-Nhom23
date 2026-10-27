using CulinaryBlog.Application.DTOs.Categories;
using MediatR;

namespace CulinaryBlog.Application.Features.Categories.Queries.GetCategories;

public sealed record GetCategoriesQuery
	: IRequest<IReadOnlyList<CategoryDto>>;

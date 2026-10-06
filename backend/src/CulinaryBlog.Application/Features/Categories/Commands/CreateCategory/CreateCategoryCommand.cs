using CulinaryBlog.Application.DTOs.Categories;
using MediatR;

namespace CulinaryBlog.Application.Features.Categories.Commands.CreateCategory;

public sealed record CreateCategoryCommand(
	string Name,
	string? Description,
	string? ImageUrl,
	int OrderIndex) : IRequest<CategoryDto>;

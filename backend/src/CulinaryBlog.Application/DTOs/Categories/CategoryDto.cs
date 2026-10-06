namespace CulinaryBlog.Application.DTOs.Categories;

public sealed record CategoryDto(
	Guid Id,
	string Name,
	string Slug,
	string? Description,
	string? ImageUrl,
	int OrderIndex,
	int RecipeCount = 0);

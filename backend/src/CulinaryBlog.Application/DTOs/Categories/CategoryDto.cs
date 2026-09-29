using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.DTOs.Recipes;

namespace CulinaryBlog.Application.DTOs.Categories;

public sealed record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? ImageUrl,
    int OrderIndex,
    int RecipeCount);

public sealed record CategoryDetailDto(
    CategoryDto Category,
    PagedResult<RecipeSummaryDto> Recipes);

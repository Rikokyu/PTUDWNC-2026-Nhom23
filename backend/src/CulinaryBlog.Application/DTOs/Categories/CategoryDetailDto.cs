using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.DTOs.Recipes;

namespace CulinaryBlog.Application.DTOs.Categories;

public sealed record CategoryDetailDto(
    CategoryDto Category,
    PagedResult<RecipeSummaryDto> Recipes);

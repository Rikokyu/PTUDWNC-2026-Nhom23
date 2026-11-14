using CulinaryBlog.Application.Common.Models;

namespace CulinaryBlog.Application.DTOs.Recipes;

public sealed record AuthorProfileDto(
    Guid Id,
    string DisplayName,
    string? AvatarUrl,
    string? Bio,
    PagedResult<RecipeSummaryDto> Recipes);

using CulinaryBlog.Application.DTOs.Categories;
using MediatR;

namespace CulinaryBlog.Application.Features.Categories.Commands.UpdateCategory;

public sealed record UpdateCategoryCommand(
    Guid Id,
    string? Name,
    string? Description,
    string? ImageUrl,
    int? OrderIndex)
    : IRequest<CategoryDto>;

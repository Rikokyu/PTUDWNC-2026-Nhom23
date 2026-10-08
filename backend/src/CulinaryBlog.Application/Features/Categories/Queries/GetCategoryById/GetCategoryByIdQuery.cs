using CulinaryBlog.Application.DTOs.Categories;
using MediatR;

namespace CulinaryBlog.Application.Features.Categories.Queries.GetCategoryById;

public sealed record GetCategoryByIdQuery(Guid Id)
    : IRequest<CategoryDto>;

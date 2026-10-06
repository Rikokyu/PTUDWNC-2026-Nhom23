using CulinaryBlog.Application.DTOs.Categories;
using MediatR;

namespace CulinaryBlog.Application.Features.Categories.Queries.GetCategoryBySlug;

public sealed record GetCategoryBySlugQuery(string Slug, int Page, int PageSize)
    : IRequest<CategoryDetailDto>;
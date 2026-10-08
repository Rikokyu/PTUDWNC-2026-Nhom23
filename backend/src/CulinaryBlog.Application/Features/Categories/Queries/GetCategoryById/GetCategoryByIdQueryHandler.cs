using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.DTOs.Categories;
using CulinaryBlog.Domain.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Features.Categories.Queries.GetCategoryById;

public sealed class GetCategoryByIdQueryHandler
    : IRequestHandler<GetCategoryByIdQuery, CategoryDto>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryByIdQueryHandler(
        ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryDto> Handle(
        GetCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var category = await _categoryRepository
            .GetActiveByIdReadOnlyAsync(
                request.Id,
                cancellationToken);

        if (category is null)
        {
            throw new NotFoundException(
                $"Category with id '{request.Id}' was not found.");
        }

        var recipeCount = await _categoryRepository
            .GetPublishedRecipeCountAsync(
                category.Id,
                cancellationToken);

        return new CategoryDto(
            category.Id,
            category.Name,
            category.Slug,
            category.Description,
            category.ImageUrl,
            category.OrderIndex,
            recipeCount);
    }
}

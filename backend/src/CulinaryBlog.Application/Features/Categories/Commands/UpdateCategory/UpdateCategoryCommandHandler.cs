using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Authorization;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.DTOs.Categories;
using CulinaryBlog.Application.Features.Categories.Queries.GetCategories;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using DomainConflictException = CulinaryBlog.Domain.Exceptions.ConflictException;

namespace CulinaryBlog.Application.Features.Categories.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandHandler
    : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly ICurrentUser _currentUser;

    public UpdateCategoryCommandHandler(
        IUnitOfWork unitOfWork,
        IMemoryCache cache,
        ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _currentUser = currentUser;
    }

    public async Task<CategoryDto> Handle(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        AuthorizationGuard.EnsureAdmin(_currentUser);

        var category = await _unitOfWork.Categories.GetActiveByIdAsync(
            request.Id,
            cancellationToken);

        if (category is null)
        {
            throw new NotFoundException(
                $"Category with id '{request.Id}' was not found.");
        }

        var name = request.Name?.Trim();

        if (name is not null
            && !string.Equals(
                name,
                category.Name,
                StringComparison.OrdinalIgnoreCase)
            && await _unitOfWork.Categories.NameExistsAsync(
                name,
                request.Id,
                cancellationToken))
        {
            throw new DomainConflictException(
                $"Category name '{name}' already exists.");
        }

        category.UpdateDetails(
            name,
            request.Description is not null,
            NormalizeOptional(request.Description),
            request.ImageUrl is not null,
            NormalizeOptional(request.ImageUrl),
            request.OrderIndex);

        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _cache.Remove(GetCategoriesQueryHandler.CacheKey);

        var recipeCount = await _unitOfWork.Categories
            .GetPublishedRecipeCountAsync(
                category.Id,
                cancellationToken);

        return ToDto(category, recipeCount);
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static CategoryDto ToDto(
        Category category,
        int recipeCount) =>
        new(
            category.Id,
            category.Name,
            category.Slug,
            category.Description,
            category.ImageUrl,
            category.OrderIndex,
            recipeCount);
}

using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Authorization;
using CulinaryBlog.Application.Common.Helpers;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.DTOs.Categories;
using CulinaryBlog.Application.Features.Categories.Queries.GetCategories;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using DomainConflictException = CulinaryBlog.Domain.Exceptions.ConflictException;

namespace CulinaryBlog.Application.Features.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandHandler
    : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly ICurrentUser _currentUser;

    public CreateCategoryCommandHandler(
        IUnitOfWork unitOfWork,
        IMemoryCache cache,
        ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _currentUser = currentUser;
    }

    public async Task<CategoryDto> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        AuthorizationGuard.EnsureAdmin(_currentUser);

        var name = request.Name.Trim();

        if (await _unitOfWork.Categories.NameExistsAsync(
                name,
                cancellationToken: cancellationToken))
        {
            throw new DomainConflictException(
                $"Category name '{name}' already exists.");
        }

        var baseSlug = SlugHelper.Generate(name);

        if (string.IsNullOrWhiteSpace(baseSlug))
        {
            throw new ValidationException(
                "Name must contain at least one letter or number.");
        }

        var slug = baseSlug;
        var suffix = 2;

        while (await _unitOfWork.Categories.SlugExistsAsync(
                   slug,
                   cancellationToken))
        {
            slug = $"{baseSlug}-{suffix++}";
        }

        var category = Category.Create(
            name,
            slug,
            NormalizeOptional(request.Description),
            NormalizeOptional(request.ImageUrl),
            request.OrderIndex);

        await _unitOfWork.Categories.AddAsync(
            category,
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _cache.Remove(GetCategoriesQueryHandler.CacheKey);

        return ToDto(category);
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static CategoryDto ToDto(Category category) =>
        new(
            category.Id,
            category.Name,
            category.Slug,
            category.Description,
            category.ImageUrl,
            category.OrderIndex,
            RecipeCount: 0);
}
